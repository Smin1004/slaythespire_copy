using UnityEngine;
using UnityEngine.EventSystems;

public enum RestOptionType
{
    Rest,
    Upgrade
}

public class RestOptionButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private RestPanelController restPanelController;
    [SerializeField] private RestOptionType optionType;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (restPanelController == null)
            return;

        if (optionType == RestOptionType.Rest)
            restPanelController.ShowRestDescription();
        else
            restPanelController.ShowUpgradeDescription();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        restPanelController?.HideDescription();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 현재는 휴식만 선택 가능하고 강화는 설명만 보여줍니다.
        if (optionType == RestOptionType.Rest)
            restPanelController?.SelectRest();
    }
}
