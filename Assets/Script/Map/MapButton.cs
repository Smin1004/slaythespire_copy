using System;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;

public class MapButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI 참조")]
    [SerializeField] private Button _buttons;      // 이 오브젝트에 붙은 Button 컴포넌트
    [SerializeField] private TMP_Text titleText; // 버튼에 표시할 텍스트
    [SerializeField] private Image _buttonImage;    // 버튼에 표시할 아이콘

    private MapNodeType nodeType;                // 이 버튼이 의미하는 노드 타입
    private Action<MapNodeType> onClicked;       // 버튼 클릭 시 실행할 콜백 함수
    private Sprite normalSprite;                // 버튼의 기본 이미지 (Inspector에서 설정하거나 Awake()에서 가져올 수 있습니다)
    private Sprite hoverSprite;                 // 버튼에 마우스가 올라갔을 때의 이미지 (Inspector에서 설정하거나 Awake()에서 가져올 수 있습니다)

    private void Awake()
    {
        // Button 참조를 Inspector에 안 넣었으면 자동으로 가져옵니다.
        if (_buttons == null)
            _buttons = GetComponent<Button>();
        if (_buttonImage == null)
            _buttonImage = GetComponent<Image>();
    }

    /// <summary>
    /// 버튼을 설정하는 함수입니다.
    /// 
    /// 호출 위치:
    /// - MapSelectView.Show()에서 호출합니다.
    /// 
    /// 매개변수:
    /// - type: 이 버튼이 나타낼 노드 타입
    /// - title: 버튼에 표시할 텍스트
    /// - icon: 버튼에 표시할 아이콘
    /// - clickAction: 버튼 클릭 시 호출할 함수
    /// </summary>
    public void Setup(MapNodeType type, string title, Sprite normal, Sprite hover, Action<MapNodeType> clickAction)
    {
        nodeType = type;
        onClicked = clickAction;

        normalSprite = normal;
        hoverSprite = hover;

        // 버튼 제목 설정
        if (titleText != null)
            titleText.text = title;

        // 버튼 이미지 설정
        if (_buttonImage != null)
            _buttonImage.sprite = normalSprite;

        if (_buttons != null)
        {
            // 기존에 등록되어 있던 클릭 이벤트를 제거합니다.
            // 같은 버튼을 재사용할 때 이벤트가 중복 실행되는 것을 막기 위해 필요합니다.
            _buttons.onClick.RemoveAllListeners();

            // 버튼을 누르면 onClicked에게 nodeType을 넘깁니다.
            // 실제로는 RunManager.SelectNode(type)가 실행됩니다.
            _buttons.onClick.AddListener(() => onClicked?.Invoke(nodeType));
        }

        gameObject.SetActive(true);
    }

    /// <summary>
    /// 사용하지 않는 선택지 버튼을 숨깁니다.
    /// 예: 이번 선택지가 2개면 3번째 버튼은 Hide() 처리됩니다.
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 마우스가 버튼 위로 올라갔을때 이벤트시스템이 자동 호출(이미지 변경)
    /// </summary>
    /// <param name="eventData"></param>
     public void OnPointerEnter(PointerEventData eventData)
    {
        if (_buttonImage != null && hoverSprite != null)
            _buttonImage.sprite = hoverSprite;
    }

    /// <summary>
    /// 마우스가 버튼 밖으로 갔을때 이벤트 자동 호출(이미지 다시 기본이미지로)
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (_buttonImage != null && normalSprite != null)
            _buttonImage.sprite = normalSprite;
    }

}