using System;
using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    private static DeckManager _instance = null;
    public static DeckManager Instance => _instance;

    [SerializeField] private List<Skill> drawPile = new();
    [SerializeField] private List<Card> handPile = new();
    [SerializeField] private List<Skill> discardPile = new();

    [SerializeField] private Card cardObj;
    [SerializeField] private AudioClip drawCardSound;
    [SerializeField] private AudioClip useCardSound;
    public Card CardPrefab => cardObj;

    public event Action<int> OnDrawPileChanged;
    public event Action<int> OnDiscardPileChanged;

    [SerializeField] private Transform DeckPos;
    [SerializeField] private Transform handCenterPoint; // Hand cards are arranged around this point.

    [SerializeField] private float defaultCardSpacing = 2f; // Base spacing between hand cards.
    [SerializeField] private float arcCurveMultiplier = 0.2f; // Vertical curve amount for hand layout.
    [SerializeField] private float arcRotationMultiplier = 5f; // Rotation amount for hand layout.

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
    }

    public void InitAwake()
    {
        _instance = this;
    }

    public void InitializeBattleDeck()
    {
        drawPile.Clear();
        handPile.Clear();
        discardPile.Clear();

        if (Player.Instance == null || Player.Instance.masterDeck == null)
        {
            NotifyPileCounts();
            return;
        }

        //임시 삽입
        foreach (Skill data in Player.Instance.masterDeck)
            drawPile.Add(new Skill(data));

        ShuffleDeck(drawPile);
        NotifyPileCounts();
    }

    private void NotifyPileCounts()
    {
        OnDrawPileChanged?.Invoke(drawPile.Count);
        OnDiscardPileChanged?.Invoke(discardPile.Count);
    }

    public void RefreshPileView()
    {
        NotifyPileCounts();
    }

    public void PlayUseCardSound()
    {
        AudioManager.Instance?.PlaySfx(useCardSound);
    }

    public void DrawCards(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            if (drawPile.Count == 0)
            {
                if (discardPile.Count == 0)
                    return;

                ReshuffleDiscardIntoDraw();
            }

            Card drawnCard = SpawnCard(DeckPos.position, DeckPos.rotation);
            if (drawnCard == null)
                return;

            drawnCard.Init(drawPile[0]);
            drawPile.RemoveAt(0);
            handPile.Add(drawnCard);
            ArrangeHandCards(handPile);
            AudioManager.Instance?.PlaySfx(drawCardSound);
        }

        NotifyPileCounts();
    }

    private Card SpawnCard(Vector3 position, Quaternion rotation)
    {
        if (cardObj == null)
            return null;

        if (ObjectPoolManager.Instance != null)
            return ObjectPoolManager.Instance.Spawn(cardObj.gameObject, position, rotation).GetComponent<Card>();

        return Instantiate(cardObj, position, rotation);
    }

    private void ReshuffleDiscardIntoDraw()
    {
        // Move all discarded cards back into the draw pile and shuffle them.
        drawPile.AddRange(discardPile);
        discardPile.Clear();
        ShuffleDeck(drawPile);
        NotifyPileCounts();
    }

    public void DiscardAllCard()
    {
        for (int i = 0; i < handPile.Count; i++)
        {
            Card card = handPile[i];
            if (card == null)
                continue;

            card.SetTargetTransform(card.targetPosition + new Vector3(15, 0), Quaternion.identity);
            card.ReturnToPoolAfterTime(1f);
            discardPile.Add(card.skill);
        }

        handPile.Clear();
        NotifyPileCounts();
    }

    public void ClearBattleCards()
    {
        foreach (Card card in handPile)
        {
            if (card == null)
                continue;

            card.CancelInvoke();
            Destroy(card.gameObject);
        }

        handPile.Clear();
        discardPile.Clear();
        drawPile.Clear();
        NotifyPileCounts();
    }

    public void AddCard(Skill skill, int pileType)
    {
        Card newCard = SpawnCard(DeckPos.position, DeckPos.rotation);
        if (newCard == null)
            return;

        newCard.Init(skill);
        newCard.targetPosition = Vector3.zero;

        switch (pileType)
        {
            case 0:
                drawPile.Add(skill);
                newCard.SetTargetTransform(newCard.targetPosition + new Vector3(-15, 0), Quaternion.identity);
                newCard.ReturnToPoolAfterTime(1f);
                break;
            case 1:
                handPile.Add(newCard);
                ArrangeHandCards(handPile);
                break;
            case 2:
                handPile.Add(newCard);
                DiscardCard(newCard);
                break;
        }

        NotifyPileCounts();
    }

    public void DiscardCard(Card card)
    {
        if (card != null && handPile.Contains(card))
        {
            card.SetTargetTransform(card.targetPosition + new Vector3(15, 0), Quaternion.identity);
            card.ReturnToPoolAfterTime(1f);
            handPile.Remove(card);
            discardPile.Add(card.skill);
            ArrangeHandCards(handPile);
        }

        NotifyPileCounts();
    }

    private void ShuffleDeck(List<Skill> list)
    {
        // Fisher-Yates shuffle keeps each card order equally likely.
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);
            Skill temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    public void ArrangeHandCards(List<Card> cardsInHand)
    {
        int cardCount = cardsInHand.Count;
        if (cardCount == 0 || handCenterPoint == null)
            return;

        float totalWidth = (cardCount - 1) * defaultCardSpacing;
        float startX = handCenterPoint.position.x - (totalWidth / 2f);

        for (int i = 0; i < cardCount; i++)
        {
            float normalizedPosition = cardCount > 1 ? (float)i / (cardCount - 1) : 0.5f;
            float distanceFromCenter = normalizedPosition - 0.5f;

            float targetX = startX + (i * defaultCardSpacing);
            float arcY = -(distanceFromCenter * distanceFromCenter) * arcCurveMultiplier;
            float targetY = handCenterPoint.position.y + arcY;
            float targetZ = handCenterPoint.position.z - (i * 0.2f);

            Vector3 calculatedTargetPos = new Vector3(targetX, targetY, targetZ);
            float targetZRotation = -distanceFromCenter * arcRotationMultiplier;
            Quaternion calculatedTargetRot = Quaternion.Euler(0, 0, targetZRotation);

            if (cardsInHand[i] != null)
                cardsInHand[i].SetTargetTransform(calculatedTargetPos, calculatedTargetRot);
        }
    }
}
