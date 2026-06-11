using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState
{
    PlayerTurnStart,
    PlayerDraw,
    PlayerAction,
    EnemyTurn,
    BattleWon
}

public class BattleManager : MonoBehaviour
{
    private static BattleManager _instance = null;
    public static BattleManager Instance => _instance;

    [Header("State")]
    [SerializeField] private BattleState curBattleState;
    [SerializeField] private Player _player;
    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private GameObject[] _hideWhenOpen;

    [Header("Enemy Spawn")]
    [SerializeField] private EnemyEntity enemyPrefab;
    [SerializeField] private EnemyData[] enemyDatas;
    [SerializeField] private Transform enemySpawnParent;
    [SerializeField] private Transform[] enemySpawnPoints;

    [Header("적 UI")]
    [SerializeField] private EnemyUI enemyUIPrefab;      // Canvas에 생성할 적 UI 프리팹입니다.
    [SerializeField] private Transform enemyUIRoot;      // EnemyUI들이 들어갈 부모입니다. Canvas/BattleRoot/EnemyUIRoot를 연결합니다.
    
    [Header("Boss Spawn")]
    [SerializeField] private bool isBossBattle;
    [SerializeField] private EnemyData bossEnemyData;

    [Header("Battle Feedback")]
    [SerializeField] private DamageSpawner damageSpawner;
    [SerializeField] private CameraShake cameraShake;
    [SerializeField] private AudioClip playerTurnStartSound;

    [SerializeField] private List<EnemyEntity> enemyList = new();
    public IReadOnlyList<EnemyEntity> EnemyList => enemyList;

    public bool isPlayerTurn;

    private void OnEnable()
    {
        if (_player != null)
            _player.OnDead += ShowGameOver;
    }

    private void OnDisable()
    {
        if (_player != null)
            _player.OnDead -= ShowGameOver;
    }

    public void InitAwake()
    {
        _instance = this;
    }

    /// <summary>
    /// TimingController.Start()에서 호출됩니다.
    /// 
    /// 이제 게임 시작 시 바로 전투를 시작하지 않습니다.
    /// 전투 시작은 RunManager가 맵 선택 후 StartNormalBattle() 또는 StartBossBattle()을 호출합니다.
    /// </summary>
    public void InitStart()
    {
        // 비워둡니다.
    }

    /// <summary>
    /// 일반 전투를 시작합니다.
    /// 
    /// 호출 위치:
    /// - RunManager.StartNormalBattle()
    /// </summary>
    public void StartNormalBattle()
    {
        isBossBattle = false;
        InitializeBattle();
    }

    /// <summary>
    /// 보스 전투를 시작합니다.
    /// 
    /// 호출 위치:
    /// - RunManager.StartBossBattle()
    /// </summary>
    public void StartBossBattle()
    {
        isBossBattle = true;
        InitializeBattle();
    }


    private void ShowGameOver()
    {
        StartCoroutine(GameOverRoutine());
    }

    private IEnumerator GameOverRoutine()
    {
        yield return new WaitForSeconds(1f);

        if (_gameOverPanel != null)
            _gameOverPanel.SetActive(true);

        SetHiddenObjects(false);
    }

    public void RestartGame()
    {
        if (_gameOverPanel != null)
            _gameOverPanel.SetActive(false);

        SetHiddenObjects(true);

        if (Player.Instance != null)
            Player.Instance.Revive();
    }

    private void SetHiddenObjects(bool active)
    {
        if (_hideWhenOpen == null)
            return;

        foreach (GameObject obj in _hideWhenOpen)
        {
            if (obj != null)
                obj.SetActive(active);
        }
    }

    private void ChangeBattleState(BattleState newState)
    {
        curBattleState = newState;

        switch (curBattleState)
        {
            case BattleState.PlayerTurnStart:
                StartPlayerTurn();
                break;
            case BattleState.PlayerDraw:
                DrawPlayerCards();
                break;
            case BattleState.PlayerAction:
                WaitForPlayerInput();
                break;
            case BattleState.EnemyTurn:
                StartCoroutine(EnemyTurnCoroutine());
                break;
            case BattleState.BattleWon:
                ProcessBattleVictory();
                break;
        }
    }

    private void InitializeBattle()
    {
        Debug.Log("Battle start");
        SpawnEnemies();

        if (DeckManager.Instance != null)
            DeckManager.Instance.InitializeBattleDeck();

        ChangeBattleState(BattleState.PlayerTurnStart);
    }

    private void SpawnEnemies()
    {
        foreach (EnemyEntity enemy in enemyList)
        {
            if (enemy != null)
                Destroy(enemy.gameObject);
        }

        enemyList.Clear();


        if (enemyPrefab == null)
        {
            Debug.LogWarning("Enemy spawn failed: enemyPrefab is null.", this);
            return;
        }

        if (isBossBattle && bossEnemyData == null)
        {
            Debug.LogWarning("Boss spawn failed: bossEnemyData is null.", this);
            return;
        }

        if (!isBossBattle && (enemyDatas == null || enemyDatas.Length == 0))
        {
            Debug.LogWarning("Enemy spawn failed: enemyDatas is empty.", this);
            return;
        }

        int spawnCount = isBossBattle ? 1 : (enemySpawnPoints != null && enemySpawnPoints.Length > 0 ? enemySpawnPoints.Length : 1);

        for (int i = 0; i < spawnCount; i++)
        {
            EnemyData selectedData = isBossBattle ? bossEnemyData : enemyDatas[Random.Range(0, enemyDatas.Length)];
            if (selectedData == null)
                continue;

            Transform spawnPoint = enemySpawnPoints != null && i < enemySpawnPoints.Length ? enemySpawnPoints[i] : null;
            Vector3 position = spawnPoint != null ? spawnPoint.position : transform.position;
            Quaternion rotation = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;
            Transform parent = enemySpawnParent != null ? enemySpawnParent : null;

            EnemyEntity enemy = Instantiate(enemyPrefab, position, rotation, parent);
            enemy.SetupEnemy(selectedData);
            enemyList.Add(enemy);

            CreateEnemyUI(enemy); // 적 UI 생성 및 연결
        }
    }

    // 적 UI 생성 및 연결
    private void CreateEnemyUI(EnemyEntity enemy)
    {
        if (enemyUIPrefab == null || enemyUIRoot == null || enemy == null)
            return;

        EnemyUI enemyUI = Instantiate(enemyUIPrefab, enemyUIRoot);
        enemyUI.Bind(enemy);
    }

    public void RemoveEnemy(EnemyEntity enemy)
    {
        if (enemy == null)
            return;

        enemyList.Remove(enemy);

        if (enemyList.Count == 0 && curBattleState != BattleState.BattleWon)
            ChangeBattleState(BattleState.BattleWon);
    }

    public void SpawnDamageText(int damage, Transform target)
    {
        if (damageSpawner != null && target != null)
            damageSpawner.SpawnDamage(damage, target);
    }

    private void StartPlayerTurn()
    {
        Debug.Log("Player turn start");

        if (_player != null)
            _player.TurnInit();

        AudioManager.Instance?.PlaySfx(playerTurnStartSound);
        ChangeBattleState(BattleState.PlayerDraw);
    }

    private void DrawPlayerCards()
    {
        Debug.Log("Draw cards");

        if (DeckManager.Instance != null)
            DeckManager.Instance.DrawCards(5);

        ChangeBattleState(BattleState.PlayerAction);
    }

    private void WaitForPlayerInput()
    {
        Debug.Log("Wait for player input");
        isPlayerTurn = true;
    }

    public void EndPlayerTurn()
    {
        if (curBattleState != BattleState.PlayerAction)
            return;

        Debug.Log("Player turn end");

        if (DeckManager.Instance != null)
            DeckManager.Instance.DiscardAllCard();

        isPlayerTurn = false;
        ChangeBattleState(BattleState.EnemyTurn);
    }

    private IEnumerator EnemyTurnCoroutine()
    {
        Debug.Log("Enemy turn");
        EnemyEntity[] enemies = enemyList.ToArray();
        foreach (EnemyEntity enemy in enemies)
        {
            if (enemy == null)
                continue;

            enemy.TurnInit();
        }

        foreach (EnemyEntity enemy in enemies)
        {
            if (enemy == null)
                continue;

            enemy.ExecuteEnemyTurn(_player);
        }

        yield return new WaitForSeconds(2f);

        // 적 버프/디버프는 적 턴이 끝날 때만 지속 턴을 줄입니다.
        foreach (EnemyEntity enemy in enemies)
        {
            yield return new WaitForSeconds(0.2f);

            if (_player != null)
                _player.TickBuffTurns();

            if (enemy == null)
                continue;

            enemy.TickBuffTurns();
        }

        if (enemyList.Count == 0)
            ChangeBattleState(BattleState.BattleWon);
        else
            ChangeBattleState(BattleState.PlayerTurnStart);
    }

    /// <summary>
    /// 전투 승리 처리.
    /// 
    /// 호출 위치:
    /// - ChangeBattleState(BattleState.BattleWon)
    /// 
    /// 처리:
    /// - 일반 전투면 RewardManager로 보상 생성
    /// - 보스 전투면 RunManager에게 보스 클리어 알림
    /// </summary>
    private void ProcessBattleVictory()
    {
        StopAllCoroutines();
        isPlayerTurn = false;

        if (isBossBattle)
        {
            RunManager.Instance?.CompleteBossBattle();
            return;
        }

        if (RewardManager.Instance != null)
        {
            RewardManager.Instance.GenerateCombatRewards();
        }
        else
        {
            // 보상 매니저가 없으면 바로 맵 선택지로 돌아갑니다.
            RunManager.Instance?.CompleteBattleReward();
        }
    }
}
