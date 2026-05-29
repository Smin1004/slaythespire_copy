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
    public bool isPlayerTurn;

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
                ExecuteEnemyTurn();
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

    private void ExecuteEnemyTurn()
    {
        Debug.Log("적 행동 실행");
        
        ChangeBattleState(BattleState.PlayerTurnStart);
    }
}