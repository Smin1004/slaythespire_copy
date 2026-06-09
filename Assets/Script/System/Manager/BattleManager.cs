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

    [Header("Battle Feedback")]
    [SerializeField] private DamageViewSpawner damageSpawner;
    [SerializeField] private CameraShake cameraShake;
    [SerializeField] private AudioManager audioManager;

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
            EnemyData selectedData = enemyDatas[Random.Range(0, enemyDatas.Length)];
            if (selectedData == null)
                continue;

            Transform spawnPoint = enemySpawnPoints != null && i < enemySpawnPoints.Length ? enemySpawnPoints[i] : null;
            Vector3 position = spawnPoint != null ? spawnPoint.position : transform.position;
            Quaternion rotation = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;
            Transform parent = enemySpawnParent != null ? enemySpawnParent : null;

            EnemyEntity enemy = Instantiate(enemyPrefab, position, rotation, parent);
            enemy.SetupEnemy(selectedData);
            enemyList.Add(enemy);
        }
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

    private void StartPlayerTurn()
    {
        Debug.Log("Player turn start");

        if (_player != null)
            _player.TurnInit();

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
