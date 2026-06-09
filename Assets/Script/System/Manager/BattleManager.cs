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
    // 모든 적이 공유하는 단일 Enemy 프리팹입니다. 적 종류 차이는 EnemyData로만 바꿉니다.
    [SerializeField] private EnemyEntity enemyPrefab;
    // 전투 시작 시 이 후보 중 하나를 랜덤 선택해서 Enemy 프리팹에 주입합니다.
    [SerializeField] private EnemyData[] enemyDatas;
    // 생성된 적들을 묶어둘 부모 Transform입니다. 비워두면 씬 루트에 생성됩니다.
    [SerializeField] private Transform enemySpawnParent;
    // 스폰 포인트 개수만큼 적을 생성합니다. 비어 있으면 BattleManager 위치에 1마리만 생성합니다.
    [SerializeField] private Transform[] enemySpawnPoints;

    [Header("Battle Feedback")]
    // 적 피격 시 데미지 숫자를 띄우는 전투 공통 스포너입니다.
    [SerializeField] private DamageViewSpawner damageSpawner;
    // Enemy 프리팹이 직접 들지 않고 BattleManager를 통해 호출하는 카메라 흔들림입니다.
    [SerializeField] private CameraShake cameraShake;
    // 적별 사운드 클립은 EnemyData에 있고, 실제 재생은 이 매니저가 담당합니다.
    [SerializeField] private AudioManager audioManager;
    // 적별 이펙트 프리팹은 EnemyData/EnemyAction에 있고, 실제 생성은 이 매니저가 담당합니다.
    // 이펙트는 아직 구조를 확정하지 않았으므로 BattleManager 연결도 잠시 사용하지 않습니다.
    // [SerializeField] private EffectManager effectManager;

    // 전투 중 살아있는 적 목록입니다. 외부에서는 읽기 전용 EnemyList로만 접근합니다.
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

    public void InitStart()
    {
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
        // 이전 전투나 씬 직렬화로 남은 적 참조가 있으면 새 스폰 구조에 맞춰 정리합니다.
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

        if (enemyDatas == null || enemyDatas.Length == 0)
        {
            Debug.LogWarning("Enemy spawn failed: enemyDatas is empty.", this);
            return;
        }

        int spawnCount = enemySpawnPoints != null && enemySpawnPoints.Length > 0 ? enemySpawnPoints.Length : 1;

        for (int i = 0; i < spawnCount; i++)
        {
            // 프리팹은 고정하고, 데이터만 랜덤으로 골라 적 종류를 결정합니다.
            EnemyData selectedData = enemyDatas[Random.Range(0, enemyDatas.Length)];
            if (selectedData == null)
                continue;

            Transform spawnPoint = enemySpawnPoints != null && i < enemySpawnPoints.Length ? enemySpawnPoints[i] : null;
            Vector3 position = spawnPoint != null ? spawnPoint.position : transform.position;
            Quaternion rotation = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;
            Transform parent = enemySpawnParent != null ? enemySpawnParent : null;

            EnemyEntity enemy = Instantiate(enemyPrefab, position, rotation, parent);
            // EnemyEntity는 전달받은 데이터로 외형/체력/행동 의도를 초기화합니다.
            enemy.SetupEnemy(selectedData);
            enemyList.Add(enemy);
        }
    }

    public void RemoveEnemy(EnemyEntity enemy)
    {
        if (enemy == null)
            return;

        enemyList.Remove(enemy);

        // 적이 모두 사라지면 기존 상태 머신의 BattleWon 흐름으로 넘깁니다.
        if (enemyList.Count == 0 && curBattleState != BattleState.BattleWon)
            ChangeBattleState(BattleState.BattleWon);
    }

    public void SpawnDamageText(int damage, Transform target)
    {
        // 현재 DamageViewSpawner API는 위치 없이도 재생 가능해서 우선 안전한 기본 호출을 사용합니다.
        if (damageSpawner != null)
            damageSpawner.SpawnDamageView(damage);
    }

    public void ShakeCamera()
    {
        if (cameraShake != null)
            cameraShake.PlayCameraShake();
    }

    public void PlaySfx(AudioClip clip)
    {
        if (clip == null)
            return;

        if (audioManager == null)
            audioManager = FindFirstObjectByType<AudioManager>();

        if (audioManager != null)
            audioManager.PlaySfx(clip);
    }

    // 이펙트는 아직 구조를 확정하지 않았으므로 잠시 사용하지 않습니다.
    // public void PlayEffect(GameObject effectPrefab, Transform target)
    // {
    //     if (effectPrefab != null && effectManager != null)
    //         effectManager.PlayEffect(effectPrefab, target);
    // }

    private void StartPlayerTurn()
    {
<<<<<<< HEAD
        Debug.Log("Player turn start");

        if (_player != null)
            _player.playerTurnInit();

=======
        Debug.Log("플레이어 턴 시작");
        _player.TurnInit();
>>>>>>> origin/main
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
            if (enemy != null)
                enemy.ExecuteEnemyTurn(_player);
        }

        yield return new WaitForSeconds(2f);

        if (enemyList.Count == 0)
            ChangeBattleState(BattleState.BattleWon);
        else
            ChangeBattleState(BattleState.PlayerTurnStart);
    }

    private void ProcessBattleVictory()
    {
        StopAllCoroutines();

        if (RewardManager.Instance != null)
            RewardManager.Instance.GenerateCombatRewards();
    }
}
