using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIButtonEffect : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    [Header("Image")]
    [SerializeField]  Image image;
    [SerializeField]  Sprite normalSprite;
    [SerializeField]  Sprite hoverSprite;

    [Header("Scale")]
    [SerializeField]  float hoverScale = 1.1f;

    [Header("Sound")]
    [SerializeField]  AudioManager soundManager;
    [SerializeField]  AudioClip hoverSound;
    [SerializeField]  AudioClip clickSound;

    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;

        // Missing inspector references are filled from local/global components.
        if (image == null)
            image = GetComponent<Image>();

        if (soundManager == null)
            soundManager = AudioManager.Instance;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = originalScale * hoverScale;

        if (image != null && hoverSprite != null)
            image.sprite = hoverSprite;

        if (soundManager != null && hoverSound != null)
            soundManager.PlaySfx(hoverSound);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = originalScale;

        if (image != null && normalSprite != null)
            image.sprite = normalSprite;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (soundManager != null && clickSound != null)
            soundManager.PlaySfx(clickSound);
    }
}
