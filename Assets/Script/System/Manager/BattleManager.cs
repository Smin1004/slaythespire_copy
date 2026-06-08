using System.Collections;
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
    [SerializeField] private Player _player;    //플레이어
    [SerializeField] private GameObject _gameOverPanel; //게임오버 패널
    [SerializeField] private GameObject[] _hideWhenOpen;  //옵션창이 열릴 때 숨길 다른 UI 요소들
    public List<EnemyEntity> enemyList; //임시 public

    public bool isPlayerTurn;
    Player player;
    
    #region //게임오버패널
    //--------------------------  게임오버 패널 관련 코드 --------------------------------------
    /// <summary>
    /// 게임오버 패널을 활성화하는 함수입니다. 플레이어가 죽었을 때 호출됩니다. 
    /// </summary>
    private void OnEnable()
    {
        _player.OnDead += ShowGameOver;
    }
    /// <summary>
    /// 게임오버 패널을 비활성화하는 함수입니다. 플레이어가 죽었을 때 호출됩니다.
    /// </summary>
    private void OnDisable()
    {
        _player.OnDead -= ShowGameOver;
    }
    /// <summary>
    /// 게임오버 패널을 활성화하는 코루틴입니다. 게임오버 패널이 활성화된 후 2초 뒤에 게임오버 패널이 사라집니다.
    /// </summary>
    private void ShowGameOver()
    {
        StartCoroutine(GameOverRoutine());
        _gameOverPanel.SetActive(true);

        SetHiddenObjects(false);
    }
    
    /// 게임오버 패널을 활성화하는 코루틴입니다. 게임오버 패널이 활성화된 후 1초 뒤에 게임오버 패널이 사라집니다.
    IEnumerator GameOverRoutine()
    {
        yield return new WaitForSeconds(1f);

        _gameOverPanel.SetActive(true);
    }
    
    // 게임오버 패널에서 다시 시작 버튼을 눌렀을 때 호출되는 함수입니다. 게임오버 패널을 닫고, 숨겼던 UI를 다시 표시하고, 플레이어를 부활시킵니다.
    public void RestartGame()
    {
        // 게임오버 창 닫기
        _gameOverPanel.SetActive(false);

        // 숨겼던 UI 다시 표시
        SetHiddenObjects(true);

        // 플레이어 부활
        Player.Instance.Revive();
    }

    // 옵션창이 열릴 때 다른 UI를 숨기는 함수입니다.
    void SetHiddenObjects(bool active)
    {
        if (_hideWhenOpen == null)
            return;

        foreach (var obj in _hideWhenOpen)
        {
            if (obj != null)
                obj.SetActive(active);
        }
    }
    #endregion


    public void InitAwake()
    {
        _instance = this;
    }

    public void InitStart()
    {
        player = Player.Instance;
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
                //EnemyTurn();
                StartCoroutine(EnemyTurnCorutin());
                break;
        }
    }

    private void InitializeBattle()
    {
        Debug.Log("전투 시작");
        DeckManager.Instance.InitializeBattleDeck();
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
        DeckManager.Instance.DrawCards(5);
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
        DeckManager.Instance.DiscardCard();
        isPlayerTurn = false;
        ChangeBattleState(BattleState.EnemyTurn);
    }

    private void EnemyTurn()
    {
        Debug.Log("적 행동 실행");
        foreach (var enemy in enemyList)
        {
            enemy.ExecuteEnemyTurn(player);
        }
        ChangeBattleState(BattleState.PlayerTurnStart);
    }

    IEnumerator EnemyTurnCorutin()
    {
        Debug.Log("적 행동 실행");
        foreach (var enemy in enemyList)
        {
            enemy.ExecuteEnemyTurn(player);
        }
        yield return new WaitForSeconds(2);
        ChangeBattleState(BattleState.PlayerTurnStart);
    }
}