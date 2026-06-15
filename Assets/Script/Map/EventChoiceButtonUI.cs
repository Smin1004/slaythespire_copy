using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EventChoiceButtonUI : MonoBehaviour
{
    [Header("UI 컴포넌트 연결")]
    [SerializeField] private TextMeshProUGUI choiceText;
    [SerializeField] private Button targetButton;

    // 매니저로부터 전달받은 클릭 실행 로직을 보관하는 변수
    private Action onClickCallback;

    // EventRoomManager에서 버튼을 생성할 때 호출하는 초기화 함수
    public void SetupButton(string buttonString, Action onClickAction)
    {
        // 1. 텍스트 적용
        if (choiceText != null)
        {
            choiceText.text = buttonString;
        }

        // 2. 콜백 로직 저장
        onClickCallback = onClickAction;

        // 3. 버튼 클릭 이벤트 리스너 세팅 (중복 실행 방지를 위해 기존 리스너 초기화)
        if (targetButton != null)
        {
            targetButton.onClick.RemoveAllListeners();
            targetButton.onClick.AddListener(OnButtonClick);
        }
    }

    // 실제 버튼이 클릭되었을 때 실행되는 내부 함수
    private void OnButtonClick()
    {
        // 저장해둔 매니저의 로직(OnChoiceSelected)을 실행합니다.
        onClickCallback?.Invoke();
    }
}