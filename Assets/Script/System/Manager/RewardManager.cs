using System.Collections.Generic;
using UnityEngine;
using System;

public enum RewardCategory
{
    GoldDrop,
    CardSelection,
    RelicDrop
}

public struct RewardItemData
{
    public RewardCategory itemCategory;
    public int goldAmount; // 골드일 경우 액수
}

public class RewardManager : MonoBehaviour
{
    public static RewardManager Instance;

    // UI 개발자(B)가 화면을 그리기 위해 구독할 이벤트
    public event Action<List<RewardItemData>> OnRewardScreenOpened;
    public event Action<List<Skill>> OnCardDraftOpened;

    public void InitAwake() { Instance = this; }

    public void GenerateCombatRewards()
    {
        List<RewardItemData> generatedRewards = new List<RewardItemData>();

        int randomGold = UnityEngine.Random.Range(10, 21);
        generatedRewards.Add(new RewardItemData { itemCategory = RewardCategory.GoldDrop, goldAmount = randomGold });

        // 2. 카드 픽 보상 생성
        generatedRewards.Add(new RewardItemData { itemCategory = RewardCategory.CardSelection });

        // 3. UI 쪽에 보상 리스트를 넘겨주며 화면을 띄우라고 지시
        Debug.Log("[Reward] 보상 목록이 생성되었습니다. UI를 호출합니다.");
        OnRewardScreenOpened?.Invoke(generatedRewards);
    }

    public void ClaimGoldReward(int amountToClaim)
    {
        // 추후 PlayerDataManager의 addGold 같은 함수 호출
        Debug.Log($"[Reward] {amountToClaim} 골드를 획득했습니다!");
    }

    public void OpenCardDraftScreen()
    {
        // 전체 카드 DB에서 랜덤으로 3장을 뽑아옵니다. (임시 로직)
        List<Skill> draftOptions = GenerateRandomDraftCards(3);
        
        Debug.Log("[Reward] 3장의 카드 선택 화면을 띄웁니다.");
        OnCardDraftOpened?.Invoke(draftOptions);
    }

    public void SelectDraftCard(Skill selectedCardData)
    {
        // 선택한 카드를 이전에 설계한 DeckManager의 마스터 덱에 영구 추가
        // DeckManager.Instance.masterDeck.Add(selectedCardData);
        Debug.Log($"[Reward] '{selectedCardData.name}' 카드를 마스터 덱에 추가했습니다!");
    }

    private List<Skill> GenerateRandomDraftCards(int optionCount)
    {
        List<Skill> options = new List<Skill>();
        // 실제로는 CardDatabase에서 등급(희귀도) 확률에 맞춰 중복 없이 뽑아오는 로직이 들어갑니다.
        return options;
    }

    // ----------------------고친부분-------------------
    public void SelectReward()
    {
        // 1. 보상 지급
        // Player.Instance.masterDeck.Add(selectedCard);

        // 2. 보상 패널 닫기
        gameObject.SetActive(false);

        // 3. 맵 선택지로 돌아가기
        RunManager.Instance?.CompleteBattleReward();
    }      
}