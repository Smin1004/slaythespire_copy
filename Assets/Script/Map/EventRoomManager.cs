using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EventRoomManager : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private Image eventImage;
    [SerializeField] private TextMeshProUGUI eventDescText;
    [SerializeField] private List<EventChoiceButtonUI> buttonGroupRoot;

    [SerializeField] private List<EventPageData> allPageData;
    [SerializeField] private EventPageData curPageData;

    public void Init()
    {
        int ranNum = Random.Range(0, allPageData.Count);
        LoadEventPage(allPageData[ranNum]);
        allPageData.Remove(allPageData[ranNum]);
    }

    // 이벤트를 처음 띄울 때 호출되는 함수
    public void LoadEventPage(EventPageData targetPage)
    {
        curPageData = targetPage;

        // 1. 이미지 및 텍스트 갱신
        if (curPageData.eventImage != null)
        {
            eventImage.sprite = curPageData.eventImage;
        }
        eventDescText.text = curPageData.dialogueText;

        // 2. 기존에 생성된 버튼들 청소
        //ClearOldButtons();

        // 3. 선택지 배열을 돌며 버튼 생성
        for (int i = 0; i < buttonGroupRoot.Count; i++)
        {
            if(i < curPageData.choiceList.Count) {
                buttonGroupRoot[i].gameObject.SetActive(true);
                CreateChoiceButton(curPageData.choiceList[i], i);
            }
            else buttonGroupRoot[i].gameObject.SetActive(false);
        }
    }

    private void CreateChoiceButton(EventChoiceData choiceData, int value)
    {
        EventChoiceButtonUI buttonUI = buttonGroupRoot[value];
        
        // 버튼 내부 텍스트 수정 및 클릭 이벤트 리스너 연결 (람다식 활용)
        buttonUI.SetupButton(choiceData.buttonText, () => OnChoiceSelected(choiceData));
    }

    // 유저가 선택지 버튼을 눌렀을 때 실행되는 함수
    private void OnChoiceSelected(EventChoiceData selectedChoice)
    {
        Debug.Log($"이벤트 선택됨: {selectedChoice.buttonText}");

        // 1. 보상/페널티 로직 실행
        ExecuteEventAction(selectedChoice.actionRewardId);

        // 2. 다음 페이지가 있다면 로드, 없다면 이벤트 종료 후 맵으로 복귀
        if (selectedChoice.targetPageData != null)
        {
            LoadEventPage(selectedChoice.targetPageData);
        }
        else
        {
            CloseEventRoom();
        }
    }

    // ID값을 읽어 실제 게임 데이터를 조작하는 관문
    private void ExecuteEventAction(string actionId)
    {
        if (string.IsNullOrEmpty(actionId) || actionId == "Action_None") return;

        // 문자열 ID를 바탕으로 해당 이름의 C# 클래스 타입을 찾습니다.
        System.Type actionType = System.Type.GetType(actionId);
        
        if (actionType != null)
        {
            // 찾은 클래스의 인스턴스를 생성하고 ExecuteAction을 트리거합니다.
            EventScript actionScript = System.Activator.CreateInstance(actionType) as EventScript;
            actionScript?.ExecuteAction(0);
        }
        else
        {
            Debug.LogError($"[EventRoom] {actionId} 클래스를 찾을 수 없습니다. 철자를 확인하세요.");
        }
    }

    private void ClearOldButtons() 
    { 
        //foreach (Transform child in buttonGroupRoot) { Destroy(child.gameObject); }
    }

    private void CloseEventRoom() 
    { 
        RunManager.Instance?.CompleteBattleReward();
    }
}