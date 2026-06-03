using System.Collections.Generic;
using UnityEngine;
using System;

public class DeckManager : MonoBehaviour
{
    private static DeckManager _instance = null;
    public static DeckManager Instance => _instance;

    [SerializeField] private List<Card> drawPile = new();
    [SerializeField] private List<Card> handPile = new();
    [SerializeField] private List<Card> discardPile = new();

    public event Action<int> OnDraw;
    public event Action<int> OnRefill;

    void Awake()
    {
        _instance = this;
    }

    // 1. 전투 시작 시 마스터 덱을 복사하여 뽑을 카드 더미 생성
    public void InitializeBattleDeck()
    {
        drawPile.Clear();
        handPile.Clear();
        discardPile.Clear();

        Debug.Log(Player.Instance.masterDeck.Count);
        foreach (var data in Player.Instance.masterDeck)
        {
            Card newCard = new Card { skill = data };
            drawPile.Add(newCard);
        }

        ShuffleDeck(drawPile);
        OnDraw?.Invoke(drawPile.Count);
    }

    // 2. 카드 뽑기 로직 (BattleManager가 호출함)
    public void DrawCards(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            if (drawPile.Count == 0)
            {
                if (discardPile.Count == 0) { Debug.Log("카드가 존재하지 않음"); return; }
                ReshuffleDiscardIntoDraw();
            }

            // 가장 위(또는 랜덤) 카드 뽑기
            Card drawnCard = drawPile[0];
            drawPile.RemoveAt(0);
            handPile.Add(drawnCard);
            //ObjectPoolManager.Instance.Spawn(drawnCard.gameObject, new Vector3(-3 + i * 2, -3), Quaternion.identity);
        }
        OnDraw?.Invoke(drawPile.Count);
    }

    // 3. 버림패를 다시 섞어 넣는 로직
    private void ReshuffleDiscardIntoDraw()
    {
        Debug.Log("[Deck] 덱이 비어 버림패를 섞습니다.");
        drawPile.AddRange(discardPile);
        discardPile.Clear();
        ShuffleDeck(drawPile);

        OnDraw?.Invoke(drawPile.Count);
        OnRefill?.Invoke(discardPile.Count);
    }

    // 리스트 셔플 (Fisher-Yates 알고리즘 등 사용)
    private void ShuffleDeck(List<Card> list) { /* ... */ }
}