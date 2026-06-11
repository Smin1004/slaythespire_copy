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
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = originalScale * hoverScale;

        if (hoverSprite != null)
            image.sprite = hoverSprite;

        if (hoverSound != null)
            soundManager.PlaySfx(hoverSound);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = originalScale;

        if (normalSprite != null)
            image.sprite = normalSprite;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (clickSound != null)
            soundManager.PlaySfx(clickSound);
    }
}