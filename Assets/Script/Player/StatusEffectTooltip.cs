using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusEffectTooltip : MonoBehaviour
{
    public static StatusEffectTooltip Instance { get; private set; }

    [SerializeField] private RectTransform root;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Vector2 screenOffset = new Vector2(16f, -16f);

    private void Awake()
    {
        Instance = this;

        if (root == null)
            root = transform as RectTransform;

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        Hide();
    }

    public static StatusEffectTooltip EnsureInstance()
    {
        if (Instance != null)
            return Instance;

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
            return null;

        GameObject rootObject = new GameObject("StatusEffectTooltip", typeof(RectTransform), typeof(CanvasGroup), typeof(Image), typeof(StatusEffectTooltip));
        rootObject.transform.SetParent(canvas.transform, false);

        RectTransform rootRect = rootObject.GetComponent<RectTransform>();
        rootRect.sizeDelta = new Vector2(220f, 72f);

        Image background = rootObject.GetComponent<Image>();
        background.color = new Color(0f, 0f, 0f, 0.82f);
        background.raycastTarget = false;

        StatusEffectTooltip tooltip = rootObject.GetComponent<StatusEffectTooltip>();
        tooltip.root = rootRect;
        tooltip.canvasGroup = rootObject.GetComponent<CanvasGroup>();
        tooltip.nameText = CreateText(rootRect, "Name", new Vector2(0f, 16f), 18);
        tooltip.descriptionText = CreateText(rootRect, "Description", new Vector2(0f, -14f), 14);
        tooltip.Hide();

        Instance = tooltip;
        return Instance;
    }

    private static TMP_Text CreateText(RectTransform parent, string objectName, Vector2 anchoredPosition, int fontSize)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        RectTransform rectTransform = textObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0f, 0.5f);
        rectTransform.anchorMax = new Vector2(1f, 0.5f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(-16f, 24f);

        TMP_Text text = textObject.GetComponent<TMP_Text>();
        text.fontSize = fontSize;
        text.color = Color.white;
        text.raycastTarget = false;

        return text;
    }

    private void Update()
    {
        if (canvasGroup != null && canvasGroup.alpha > 0f)
            SetPosition(Input.mousePosition);
    }

    public void Show(Buff buff, Vector2 screenPosition)
    {
        if (buff == null)
            return;

        if (nameText != null)
            nameText.text = buff.name;

        if (descriptionText != null)
            descriptionText.text = buff.desc;

        SetPosition(screenPosition);

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
    }

    private void SetPosition(Vector2 screenPosition)
    {
        if (root != null)
            root.position = screenPosition + screenOffset;
    }
}
