using System.Collections.Generic;
using UnityEngine;

public enum BattleState
{
    PlayerTurnStart,
    PlayerDraw,
    PlayerAction,
    EnemyTurn
}

public class BattleManager : MonoBehaviour
{
    private static BattleManager _instance = null;
    public static BattleManager Instance => _instance;

    [SerializeField] private BattleState curBattleState;
    public List<EnemyEntity> enemyList; //임시 public
    
    public List<Card> curDeck;
    public List<Card> useDeck;
    public List<Card> Deck;

    [Header("Effect References")]
    [SerializeField] private CameraShack cameraShack;
    [SerializeField] private soundManager soundManager;
    [SerializeField] private DamagePopupSpawner damagePopupSpawner;
    [SerializeField] private AudioClip playerAttackSfx;
    [SerializeField] private AudioClip playerHitSfx;
    [SerializeField] private float damagePopupYOffset = 2f;

    public bool isPlayerTurn;
    Player player;

    public void _Instance()
    {
        _instance = this;
    }

    void Awake()
    {
        _Instance();
    }

    private void Start()
    {
        player = Player.Instance;
        if (player == null)
        {
            Debug.LogError("BattleManager: Player.Instance가 null입니다. Player 오브젝트가 씬에 없거나 Awake가 호출되지 않았습니다.");
        }

        if (cameraShack == null)
        {
            Debug.LogWarning("BattleManager: CameraShack 참조가 할당되지 않았습니다.");
        }
        if (soundManager == null)
        {
            Debug.LogWarning("BattleManager: SoundManager 참조가 할당되지 않았습니다.");
        }
        if (damagePopupSpawner == null)
        {
            Debug.LogWarning("BattleManager: DamagePopupSpawner 참조가 할당되지 않았습니다.");
        }

        InitializeBattle();
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
                EnemyTurn();
                break;
        }
    }

    private void InitializeBattle()
    {
        Debug.Log("전투 시작");
        ChangeBattleState(BattleState.PlayerTurnStart);
    }

    private void StartPlayerTurn()
    {
        Debug.Log("플레이어 턴 시작");
        player.playerTurnInit();
        ChangeBattleState(BattleState.PlayerDraw);
    }

    private void DrawPlayerCards()
    {
        Debug.Log("카드 드로우");
        ChangeBattleState(BattleState.PlayerAction);
    }

    private void WaitForPlayerInput()
    {
        Debug.Log("플레이어 행동 대기");
        isPlayerTurn = true;
        // 여기서 코드는 멈추고 유저의 조작(PlayCard, EndPlayerTurn)을 기다립니다.
    }

    // 턴 종료 버튼을 눌렀을 때 호출되는 함수
    public void EndPlayerTurn()
    {
        if (curBattleState != BattleState.PlayerAction) return;
        Debug.Log("플레이어 턴 종료");
        isPlayerTurn = false;
        ChangeBattleState(BattleState.EnemyTurn);
    }

    private void EnemyTurn()
    {
        Debug.Log("적 행동 실행");

        if (enemyList == null || enemyList.Count == 0)
        {
            Debug.LogWarning("BattleManager: enemyList가 비어있습니다. 적 턴을 건너뜁니다.");
            ChangeBattleState(BattleState.PlayerTurnStart);
            return;
        }

        if (player == null)
        {
            Debug.LogError("BattleManager: player가 null입니다. 적 턴을 실행할 수 없습니다.");
            return;
        }

        foreach (var enemy in enemyList)
        {
            if (enemy == null)
            {
                Debug.LogWarning("BattleManager: enemyList에 null 항목이 포함되어 있습니다.");
                continue;
            }

            int damageDone = enemy.ExecuteEnemyTurn(player);
            if (damageDone > 0)
            {
                SpawnDamageOnTarget(player.transform.position, damageDone, true);
            }
        }

        ChangeBattleState(BattleState.PlayerTurnStart);
    }

    public void PlayerAttackEnemy(EnemyEntity enemy, int damageAmount)
    {
        if (enemy == null)
        {
            Debug.LogWarning("BattleManager: 공격 대상 Enemy가 없습니다.");
            return;
        }

        int damageDone = enemy.Damage(damageAmount);
        if (damageDone > 0)
        {
            SpawnDamageOnTarget(enemy.transform.position, damageDone, false);
        }

        cameraShack?.PlayAttackShake();
        if (soundManager != null && playerAttackSfx != null)
        {
            soundManager.PlaySFX(playerAttackSfx);
        }

        Debug.Log($"플레이어가 {enemy.name}에게 {damageDone} 데미지 입힘.");
    }

    private void SpawnDamageOnTarget(Vector3 worldPosition, int damage, bool isPlayerHit)
    {
        if (damage <= 0) return;

        if (damagePopupSpawner != null)
        {
            damagePopupSpawner.SpawnDamageText(worldPosition + Vector3.up * damagePopupYOffset, damage);
        }

        if (isPlayerHit)
        {
            cameraShack?.PlayHitEffect();
            if (soundManager != null && playerHitSfx != null)
            {
                soundManager.PlaySFX(playerHitSfx);
            }
        }
    }
}