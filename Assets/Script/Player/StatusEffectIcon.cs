using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StatusEffectIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private TMP_FontAsset valueFont;
    [SerializeField] private StatusEffectTooltip tooltip;

    private Buff buff;

    private void Awake()
    {
        if (iconImage == null)
            iconImage = GetComponent<Image>();

        if (iconImage == null)
            iconImage = gameObject.AddComponent<Image>();

        iconImage.raycastTarget = true;

        if (valueText == null)
            valueText = CreateValueText();

        ApplyFont();
    }

    public void Setup(Buff targetBuff)
    {
        buff = targetBuff;

        if (iconImage != null)
            iconImage.sprite = buff != null ? buff.img : null;

        if (valueText != null)
            valueText.text = buff != null && buff.value > 0 ? buff.value.ToString() : "";
    }

    public void Setup(Buff targetBuff, StatusEffectTooltip assignedTooltip)
    {
        tooltip = assignedTooltip;
        Setup(targetBuff);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (buff != null && tooltip != null)
            tooltip.Show(buff);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltip != null)
            tooltip.Hide();
    }

    private TMP_Text CreateValueText()
    {
        GameObject textObject = new GameObject("Value", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(transform, false);

        RectTransform rectTransform = textObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(1f, 0f);
        rectTransform.anchorMax = new Vector2(1f, 0f);
        rectTransform.pivot = new Vector2(1f, 0f);
        rectTransform.anchoredPosition = new Vector2(2f, -2f);
        rectTransform.sizeDelta = new Vector2(34f, 24f);

        TMP_Text text = textObject.GetComponent<TMP_Text>();
        text.alignment = TextAlignmentOptions.BottomRight;
        text.fontSize = 22f;
        text.color = Color.white;
        text.raycastTarget = false;
        text.text = "";

        return text;
    }

    private void ApplyFont()
    {
        if (valueFont == null)
            valueFont = Resources.Load<TMP_FontAsset>("Front/NanumGothic SDF");

        if (valueFont != null && valueText != null)
            valueText.font = valueFont;
    }
}
