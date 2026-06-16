using System.Collections.Generic;
using UnityEngine;

// 이 줄 덕분에 프로젝트 창에서 우클릭 -> Create -> GameData -> EventPage로 파일을 생성할 수 있습니다.
[CreateAssetMenu(fileName = "NewEventPage", menuName = "GameData/EventPage")]
public class EventPageData : ScriptableObject
{
    [Header("이벤트 시각 정보")]
    [TextArea(5, 10)] // 인스펙터에서 텍스트 창을 넓게 쓰기 위한 속성입니다.
    public string dialogueText;
    public Sprite eventImage;

    [Header("선택지 목록")]
    public List<EventChoiceData> choiceList = new List<EventChoiceData>();
}

// 이 구조체는 EventPageData 안에 종속되어 인스펙터에 리스트 형태로 나타납니다.
[System.Serializable]
public class EventChoiceData
{
    [Header("UI 표시 텍스트")]
    public string buttonText; // 예: "치료 [35 골드 상실, 체력 25% 회복]"
    
    [Header("결과 (다음 페이지)")]
    [Tooltip("이 버튼을 누르면 이동할 다음 이벤트 페이지입니다. 비워두면 이벤트가 즉시 종료됩니다.")]
    public EventPageData targetPageData; 
    
    [Header("발동할 스크립트 ID")]
    [Tooltip("체력을 깎거나 카드를 주는 등의 실제 데이터를 변경할 식별자입니다.")]
    public string actionRewardId; // 예: "Action_ClericHeal"
    public int value;
}