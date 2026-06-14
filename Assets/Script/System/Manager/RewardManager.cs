using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum RewardCategory
{
    GoldDrop,
    CardSelection,
    RelicDrop
}

public struct RewardItemData
{
    public RewardCategory itemCategory;
    public int goldAmount; // Gold reward amount.
}

public class RewardManager : MonoBehaviour
{
    public static RewardManager Instance;

    public event Action<List<RewardItemData>> OnRewardScreenOpened;
    public event Action<List<Skill>> OnCardDraftOpened;

    [Header("Reward UI")]
    [SerializeField] private GameObject rewardChoiceButton;
    [SerializeField] private GameObject continueButton;
    [SerializeField] private Transform rewardUiRoot;

    private readonly List<GameObject> draftObjects = new();
    private List<Skill> currentDraftOptions = new();
    private bool isCardSelected;

    public void InitAwake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        BindRewardUi();
        ResetRewardUi();
    }

    public void GenerateCombatRewards()
    {
        BindRewardUi();
        ResetRewardUi();

        List<RewardItemData> generatedRewards = new List<RewardItemData>();

        int randomGold = UnityEngine.Random.Range(10, 21);
        generatedRewards.Add(new RewardItemData { itemCategory = RewardCategory.GoldDrop, goldAmount = randomGold });

        // A normal combat reward opens one card draft.
        generatedRewards.Add(new RewardItemData { itemCategory = RewardCategory.CardSelection });

        Debug.Log("[Reward] Rewards generated.");
        OnRewardScreenOpened?.Invoke(generatedRewards);
    }

    public void ClaimGoldReward(int amountToClaim)
    {
        Debug.Log($"[Reward] Gold gained: {amountToClaim}");
    }

    public void OpenCardDraftScreen()
    {
        BindRewardUi();
        ClearDraftObjects();

        currentDraftOptions = GenerateRandomDraftCards(3);
        isCardSelected = false;

        if (rewardChoiceButton != null)
            rewardChoiceButton.SetActive(false);

        if (continueButton != null)
            continueButton.SetActive(false);

        if (currentDraftOptions.Count == 0)
        {
            isCardSelected = true;

            if (continueButton != null)
                continueButton.SetActive(true);

            Debug.LogWarning("[Reward] No card data found in LoadData.");
            return;
        }

        CreateDraftButtons(currentDraftOptions);

        Debug.Log("[Reward] Card draft opened.");
        OnCardDraftOpened?.Invoke(currentDraftOptions);
    }

    public void SelectDraftCard(Skill selectedCardData)
    {
        if (selectedCardData == null)
            return;

        // Add the chosen reward card to the player's master deck.
        if (Player.Instance != null && Player.Instance.masterDeck != null)
            Player.Instance.masterDeck.Add(selectedCardData);

        isCardSelected = true;
        ClearDraftObjects();

        if (continueButton != null)
            continueButton.SetActive(true);

        Debug.Log($"[Reward] Card added: {selectedCardData.name}");
    }

    public void SelectReward()
    {
        BindRewardUi();

        if (transform.parent != null && !transform.parent.gameObject.activeInHierarchy)
        {
            RunManager.Instance?.CompleteBattleReward();
            return;
        }

        if (!isCardSelected)
        {
            OpenCardDraftScreen();
            return;
        }

        RunManager.Instance?.CompleteBattleReward();
    }

    private List<Skill> GenerateRandomDraftCards(int optionCount)
    {
        List<Skill> options = new List<Skill>();
        List<Skill> source = DataManager.Instance != null && DataManager.Instance.loadData != null
            ? DataManager.Instance.loadData.SkillList
            : null;

        if (source == null || source.Count == 0)
            return options;

        List<Skill> pool = new List<Skill>(source);
        int count = Mathf.Min(optionCount, pool.Count);

        // Pick unique random cards from LoadData.
        for (int i = 0; i < count; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, pool.Count);
            options.Add(pool[randomIndex]);
            pool.RemoveAt(randomIndex);
        }

        return options;
    }

    private void ResetRewardUi()
    {
        isCardSelected = false;
        ClearDraftObjects();

        if (rewardChoiceButton != null)
            rewardChoiceButton.SetActive(true);

        if (continueButton != null)
            continueButton.SetActive(false);
    }

    private void BindRewardUi()
    {
        Transform panelRoot = transform.parent;
        if (panelRoot == null)
            return;

        if (rewardUiRoot == null)
            rewardUiRoot = FindChildByName(panelRoot, "RewardUi");

        if (rewardChoiceButton == null)
            rewardChoiceButton = FindChildByName(panelRoot, "Choise_button")?.gameObject;

        if (continueButton == null)
            continueButton = FindChildByName(panelRoot, "continue_button")?.gameObject;

        Button choiceButton = rewardChoiceButton != null ? rewardChoiceButton.GetComponent<Button>() : null;
        if (choiceButton != null)
        {
            choiceButton.onClick.RemoveListener(SelectReward);
            choiceButton.onClick.AddListener(SelectReward);
        }
    }

    private Transform FindChildByName(Transform root, string childName)
    {
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == childName)
                return child;
        }

        return null;
    }

    private void CreateDraftButtons(List<Skill> draftOptions)
    {
        if (rewardUiRoot == null || draftOptions == null)
            return;

        GameObject draftRoot = new GameObject("CardDraftOptions", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        draftRoot.transform.SetParent(rewardUiRoot, false);
        draftObjects.Add(draftRoot);

        RectTransform rootRect = draftRoot.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 0.5f);
        rootRect.anchorMax = new Vector2(0.5f, 0.5f);
        rootRect.anchoredPosition = new Vector2(0f, 45f);
        rootRect.sizeDelta = new Vector2(900f, 360f);

        HorizontalLayoutGroup layout = draftRoot.GetComponent<HorizontalLayoutGroup>();
        layout.spacing = 35f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;

        foreach (Skill skill in draftOptions)
            CreateDraftCardButton(draftRoot.transform, skill);
    }

    private void CreateDraftCardButton(Transform parent, Skill skill)
    {
        Card cardPrefab = DeckManager.Instance != null ? DeckManager.Instance.CardPrefab : null;
        if (cardPrefab == null)
            return;

        GameObject cardObject = new GameObject("RewardCard", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        cardObject.transform.SetParent(parent, false);
        draftObjects.Add(cardObject);

        RectTransform rect = cardObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(250f, 340f);

        LayoutElement layoutElement = cardObject.GetComponent<LayoutElement>();
        layoutElement.preferredWidth = 250f;
        layoutElement.preferredHeight = 340f;

        Image frameImage = cardObject.GetComponent<Image>();
        frameImage.color = Color.white;
        frameImage.sprite = cardPrefab.GetComponent<SpriteRenderer>()?.sprite;
        frameImage.preserveAspect = false;

        Button button = cardObject.GetComponent<Button>();
        button.onClick.AddListener(() => SelectDraftCard(skill));

        CreateArtwork(cardObject.transform, skill);
        CreateCardText(cardObject.transform, cardPrefab.nameText, skill != null ? skill.name : "Unknown", new Vector2(0f, 120f), new Vector2(200f, 42f), 24f);
        CreateCardText(cardObject.transform, cardPrefab.costText, skill != null ? skill.cost.ToString() : "0", new Vector2(-88f, 128f), new Vector2(45f, 45f), 28f);
        CreateCardText(cardObject.transform, cardPrefab.descText, skill != null ? skill.desc : "", new Vector2(0f, -105f), new Vector2(205f, 95f), 18f);
    }

    private void CreateArtwork(Transform parent, Skill skill)
    {
        GameObject artObject = new GameObject("CardArtwork", typeof(RectTransform), typeof(Image));
        artObject.transform.SetParent(parent, false);

        RectTransform rect = artObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, 35f);
        rect.sizeDelta = new Vector2(165f, 105f);

        Image image = artObject.GetComponent<Image>();
        image.color = Color.white;
        image.sprite = skill != null ? skill.img : null;
        image.preserveAspect = true;
    }

    private void CreateCardText(Transform parent, TextMeshPro template, string value, Vector2 position, Vector2 size, float fontSize)
    {
        GameObject textObject = new GameObject("CardText", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.text = value;
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = fontSize;
        text.color = Color.white;

        if (template != null)
            text.font = template.font;
    }

    private void ClearDraftObjects()
    {
        foreach (GameObject draftObject in draftObjects)
        {
            if (draftObject != null)
                Destroy(draftObject);
        }

        draftObjects.Clear();
    }
}
