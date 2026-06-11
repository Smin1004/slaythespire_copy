using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// 맵에서 선택할 수 있는 노드 종류입니다.
/// 
/// 사용 위치:
/// - RunManager: 랜덤 선택지를 만들 때 사용
/// - MapSelectView: 선택지 버튼에 표시할 이름/아이콘을 정할 때 사용
/// - MapChoiceButton: 버튼이 어떤 노드인지 저장할 때 사용
/// </summary>
public enum MapNodeType
{
    Battle, // 일반 전투
    Elite,  // 엘리트 전투
    Event,  // 랜덤 이벤트
    Rest,   // 휴식
    Boss    // 보스 전투
}

/// - 현재 진행도 표시
/// - 선택지 버튼 2~3개 표시
/// - 노드 타입에 따라 이름과 아이콘을 다르게 표시
/// 
/// 참조 관계:
/// - RunManager가 MapSelectView.Show()를 호출합니다.
/// - MapSelectView는 MapButton.Setup()을 호출합니다.
/// - MapButton이 클릭되면 RunManager.SelectNode()가 실행됩니다.
/// </summary>
public class MapSelectView : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject root; // MapSelectPanel 전체 오브젝트

    [Header("UI")]
    [SerializeField] private TMP_Text depthText;              // 진행도 표시 텍스트
    [Header("플레이어 상태")]
    [SerializeField] private Player player;          // 현재 플레이어입니다. Inspector에서 Player 오브젝트를 연결합니다.
    [SerializeField] private TMP_Text playerHpText;  // 맵 선택 화면에 표시할 플레이어 HP 텍스트입니다.
    
    [SerializeField] private MapButton[] choiceButtons; // 선택지 버튼 배열

    // 노드 타입별 이미지 (Inspector에서 설정)
    [Header("Normal Sprites")]
    [SerializeField] private Sprite battleSprite;
    // [SerializeField] private Sprite eliteSprite;
    [SerializeField] private Sprite eventSprite;
    [SerializeField] private Sprite restSprite;
    [SerializeField] private Sprite bossSprite;

    [Header("Hover Sprites")]
    [SerializeField] private Sprite battleHoverSprite;
    // [SerializeField] private Sprite eliteHoverSprite;
    [SerializeField] private Sprite eventHoverSprite;
    [SerializeField] private Sprite restHoverSprite;
    [SerializeField] private Sprite bossHoverSprite;
    private void Awake()
    {
        // root를 비워두면 자기 자신을 패널 루트로 사용합니다.
        if (root == null)
            root = gameObject;
    }

    /// <summary>
    /// 맵 선택지를 화면에 보여주는 함수입니다.
    /// 
    /// 호출 위치:
    /// - RunManager.ShowNextChoices()
    /// 
    /// 매개변수:
    /// - choices: 이번에 표시할 선택지 목록
    /// - currentDepth: 현재 진행도
    /// - bossDepth: 보스까지 필요한 진행도
    /// - onSelect: 선택지 클릭 시 호출할 함수
    /// </summary>
    public void Show(
        List<MapNodeType> choices,
        int currentDepth,
        int bossDepth,
        Action<MapNodeType> onSelect)
    {
        if (root != null)
            root.SetActive(true);

        if (depthText != null)
            depthText.text = $"진행도 {currentDepth} / {bossDepth}";

        UpdatePlayerHpText();
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            // 선택지 개수 안에 들어오면 버튼을 세팅합니다.
            if (i < choices.Count)
            {
                MapNodeType type = choices[i];

                choiceButtons[i].Setup(
                    type,
                    GetTitle(type),
                    GetNormalSprite(type),
                    GetHoverSprite(type),
                    onSelect
                );
            }
            // 선택지 개수보다 버튼이 많으면 남는 버튼은 숨깁니다.
            else
            {
                choiceButtons[i].Hide();
            }
        }
    }

    /// <summary>
    /// 맵 선택 패널을 숨깁니다.
    /// 호출 위치:
    /// - RunManager.HideAllPanels()
    /// - RunManager가 다른 콘텐츠로 넘어갈 때
    /// </summary>
    public void Hide()
    {
        if (root != null)
            root.SetActive(false);
    }

    /// <summary>
    /// 노드 타입에 따라 버튼에 표시할 이름을 반환합니다.
    /// </summary>
    private string GetTitle(MapNodeType type)
    {
        switch (type)
        {
            case MapNodeType.Battle:
                return "일반 전투";

            // case MapNodeType.Elite:
            //     return "엘리트 전투";

            case MapNodeType.Event:
                return "이벤트";

            case MapNodeType.Rest:
                return "휴식";

            case MapNodeType.Boss:
                return "보스 전투";

            default:
                return "알 수 없음";
        }
    }

    /// <summary>
    /// 노드 타입에 따라 버튼에 표시할 아이콘을 반환합니다.
    /// </summary>
    private Sprite GetNormalSprite(MapNodeType type)
    {
        switch (type)
        {
            case MapNodeType.Battle:
                return battleSprite;
            // case MapNodeType.Elite:
                // return eliteSprite;
            case MapNodeType.Event:
                return eventSprite;
            case MapNodeType.Rest:
                return restSprite;
            case MapNodeType.Boss:
                return bossSprite;
            default:
                return null;
        }
    }
    private Sprite GetHoverSprite(MapNodeType type)
    {
        switch (type)
        {
            case MapNodeType.Battle:
                return battleHoverSprite;
            //case MapNodeType.Elite:
                // return eliteHoverSprite;
            case MapNodeType.Event:
                return eventHoverSprite;
            case MapNodeType.Rest:
                return restHoverSprite;
            case MapNodeType.Boss:
                return bossHoverSprite;
            default:
                return null;
        }
    }
    private void UpdatePlayerHpText()
    {
        if (player == null || playerHpText == null)
            return;

        playerHpText.text = $"HP {player.curHp} / {player.maxHp}";
    }
}