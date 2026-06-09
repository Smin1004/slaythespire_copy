using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StatusEffectIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text valueText;

    private Buff buff;

    private void Awake()
    {
        if (iconImage == null)
            iconImage = GetComponent<Image>();
    }

    public void Setup(Buff targetBuff)
    {
        buff = targetBuff;

        if (iconImage != null)
            iconImage.sprite = buff != null ? buff.img : null;

        if (valueText != null)
            valueText.text = buff != null && buff.value > 0 ? buff.value.ToString() : "";
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (buff != null)
            StatusEffectTooltip.EnsureInstance()?.Show(buff, eventData.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StatusEffectTooltip.Instance?.Hide();
    }
}
