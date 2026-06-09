using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UIElements;

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

    public event Action<int> OnDrawPileChanged;
    public event Action<int> OnDiscardPileChanged;

    [SerializeField] private Transform testPos;

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

    Skill tempSkill = new Skill{type = SkillType.Attack};
    // public void UpdateDesc()
    // {
    //     int[] tempArr;
    //     int temp;
    //     foreach (var n in handPile)
    //     {
    //         tempArr = n.skill.isUpgraded ? n.skill.upgradeValue : n.skill.skillValue;
    //         temp = Player.Instance.BuffCheck_CardTrigger(n.skill, tempArr[0]);
    //         temp = Player.Instance.BuffCheck_Attack(Player.Instance, temp);
    //         temp = Player.Instance.target.BuffCheck_Block(target, temp);
    //         n.skill.desc = n.skill.effect.FormatDesc(n.skill);
    //     }
    // }

    // 1. 전투 시작 시 마스터 덱을 복사하여 뽑을 카드 더미 생성
    public void InitializeBattleDeck()
    {
        drawPile.Clear();
        handPile.Clear();
        discardPile.Clear();

        Debug.Log(Player.Instance.masterDeck.Count);
        foreach (var data in Player.Instance.masterDeck)
        {
            drawPile.Add(data);
        }
        ShuffleDeck(drawPile);
        NotifyPileCounts();

    }

    private void NotifyPileCounts()
    {
        Debug.Log($"[DeckManager] 덱 UI 갱신 요청 / Draw:{drawPile.Count}, Discard:{discardPile.Count}");
        OnDrawPileChanged?.Invoke(drawPile.Count);
        OnDiscardPileChanged?.Invoke(discardPile.Count);
    }

        public void RefreshPileView()
        {
            NotifyPileCounts();
        }

        public void PlayUseCardSound()
        {
            BattleManager.Instance?.PlaySfx(useCardSound);
        }

    //드로우
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
                Card drawnCard = ObjectPoolManager.Instance.Spawn(cardObj.gameObject, testPos.position, testPos.rotation).GetComponent<Card>();
                drawnCard.Init(drawPile[0]);
                drawPile.RemoveAt(0);
                handPile.Add(drawnCard);
                ArrangeHandCards(handPile);
                BattleManager.Instance?.PlaySfx(drawCardSound);
            }
            NotifyPileCounts();
        }

    //패섞기
    private void ReshuffleDiscardIntoDraw()
    {
        Debug.Log("패섞기");
        drawPile.AddRange(discardPile);
        discardPile.Clear();
        ShuffleDeck(drawPile);

        NotifyPileCounts();
    }

    public void DiscardAllCard()
    {
        for (int i = 0; i < handPile.Count; i++)
        {
            Card temp = handPile[i];
            temp.SetTargetTransform(temp.targetPosition + new Vector3(15, 0), new Quaternion(0, 0, 0, 0));
            temp.ReturnToPoolAfterTime(1f);
            discardPile.Add(temp.skill);
        }
        handPile.Clear();
        NotifyPileCounts();
    }

    public void DiscardCard(Card card)
    {
        if (handPile.Contains(card))
        {
            card.SetTargetTransform(card.targetPosition + new Vector3(15, 0), new Quaternion(0, 0, 0, 0));
            card.ReturnToPoolAfterTime(1f);
            handPile.Remove(card);
            discardPile.Add(card.skill);
            ArrangeHandCards(handPile);
        }
        NotifyPileCounts();
    }

    // 리스트 셔플 (Fisher-Yates 알고리즘 등 사용)
    private void ShuffleDeck(List<Skill> list) { /* ... */ }



    [SerializeField] private Transform handCenterPoint; // 화면 하단 중앙의 빈 오브젝트 위치
    [SerializeField] private float defaultCardSpacing = 2f; // 카드 사이의 기본 간격
    [SerializeField] private float arcCurveMultiplier = 0.2f; // 부채꼴로 휘어지는 정도
    [SerializeField] private float arcRotationMultiplier = 5f; // 부채꼴로 회전하는 각도

    public void ArrangeHandCards(List<Card> cardsInHand)
    {
        int cardCount = cardsInHand.Count;
        if (cardCount == 0) return;

        // 1. 전체 너비와 시작점 계산
        float totalWidth = (cardCount - 1) * defaultCardSpacing;
        float startX = handCenterPoint.position.x - (totalWidth / 2f);

        for (int i = 0; i < cardCount; i++)
        {
            // 중심으로부터 얼마나 떨어져 있는지 비율 계산 (-1.0 ~ 1.0)
            float normalizedPosition = (cardCount > 1) ? (float)i / (cardCount - 1) : 0.5f;
            float distanceFromCenter = normalizedPosition - 0.5f;

            // 2. X 좌표 배치
            float targetX = startX + (i * defaultCardSpacing);

            // 3. Y 좌표 배치 (중앙은 높고, 양끝은 낮아지는 2차 함수 부채꼴 포물선)
            // distanceFromCenter의 제곱을 빼주어 아치형을 만듭니다.
            float arcY = -(distanceFromCenter * distanceFromCenter) * arcCurveMultiplier;
            float targetY = handCenterPoint.position.y + arcY;

            // Z 겹침 방지 (오른쪽 카드가 미세하게 더 앞에 오도록)
            float targetZ = handCenterPoint.position.z - (i * 0.2f);

            Vector3 calculatedTargetPos = new Vector3(targetX, targetY, targetZ);

            // 4. Z축 회전 배치 (양 끝으로 갈수록 부채꼴처럼 눕혀짐)
            float targetZRotation = -distanceFromCenter * arcRotationMultiplier;
            Quaternion calculatedTargetRot = Quaternion.Euler(0, 0, targetZRotation);

            // 5. 각 카드에게 새로운 목표 하달
            cardsInHand[i].SetTargetTransform(calculatedTargetPos, calculatedTargetRot);
        }
    }
}