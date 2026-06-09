using TMPro;
using UnityEngine;

public class StatusEffectTooltip : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_FontAsset tooltipFont;

    private void Awake()
    {
        if (root == null)
            root = gameObject;

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        ApplyFont();
        Hide();
    }

    public void Show(Buff buff)
    {
        if (buff == null)
            return;

        ApplyFont();

        if (nameText != null)
            nameText.text = buff.name;

        if (descriptionText != null)
            descriptionText.text = FormatDescription(buff);

        if (root != null)
            root.SetActive(true);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
    }

    public void Hide()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        if (root != null)
            root.SetActive(false);
    }

    private string FormatDescription(Buff buff)
    {
        if (buff == null || string.IsNullOrEmpty(buff.desc))
            return "";

        try
        {
            return string.Format(buff.desc, buff.value);
        }
        catch (System.FormatException)
        {
            return buff.desc;
        }
    }

    private void ApplyFont()
    {
        if (tooltipFont == null)
            tooltipFont = Resources.Load<TMP_FontAsset>("Front/NanumGothic SDF");

        if (tooltipFont == null)
            return;

        if (nameText != null)
            nameText.font = tooltipFont;

        if (descriptionText != null)
            descriptionText.font = tooltipFont;
    }
}
