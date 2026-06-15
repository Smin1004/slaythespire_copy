using TMPro;
using UnityEngine;

public class DeckPileView : MonoBehaviour
{
    [SerializeField] private TMP_Text drawPileText;
    [SerializeField] private TMP_Text discardPileText;


    private void Awake()
    {
        //Debug.Log($"[DeckPileView] drawPileText �Ҵ��? {drawPileText != null}");
        //Debug.Log($"[DeckPileView] discardPileText �Ҵ��? {discardPileText != null}");
    }

    private void Start()
    {
        if (DeckManager.Instance == null)
        {
            Debug.LogError("DeckManager.Instance null");
            return;
        }

        DeckManager.Instance.OnDrawPileChanged += UpdateDrawPile;
        DeckManager.Instance.OnDiscardPileChanged += UpdateDiscardPile;

        DeckManager.Instance.RefreshPileView();
    }

    private void OnDestroy()
    {
        if (DeckManager.Instance == null)
            return;

        DeckManager.Instance.OnDrawPileChanged -= UpdateDrawPile;
        DeckManager.Instance.OnDiscardPileChanged -= UpdateDiscardPile;
    }

    private void UpdateDrawPile(int count)
    {
        //Debug.Log($"[DeckPileView] Draw UI ����: {count}");
        drawPileText.text = count.ToString();
    }

    private void UpdateDiscardPile(int count)
    {
        //Debug.Log($"[DeckPileView] Discard UI ����: {count}");
        discardPileText.text = count.ToString();
    }
}