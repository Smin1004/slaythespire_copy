using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RestPanelController : MonoBehaviour
{
    [Header("Options")]
    [SerializeField] private GameObject restOption;
    [SerializeField] private GameObject upgradeOption;
    [SerializeField] private GameObject continueButton;

    [Header("Description")]
    [SerializeField] private GameObject descRoot;
    [SerializeField] private TMP_Text descText;
    [SerializeField] private TMP_Text messageText;

    [Header("Rest")]
    [SerializeField] private float healRatio = 0.3f;
    [SerializeField] private AudioClip healSound;

    [Header("Upgrade")]
    [SerializeField] ViewCard viewCard;
    [SerializeField] List<ViewCard> cards = new();
    [SerializeField] GameObject showCard;
    [SerializeField] GameObject panels;
    [SerializeField] RectTransform btnParent;

    private const string RestDescription = "최대 체력의 30%를 회복합니다.";
    private const string UpgradeDescription = "카드 한 장을 강화합니다.";

    Player player;

    private void OnEnable()
    {
        player = Player.Instance;
        ResetView();
    }

    public void ShowRestDescription()
    {
        ShowDescription(RestDescription);
    }

    public void ShowUpgradeDescription()
    {
        ShowDescription(UpgradeDescription);
    }

    public void HideDescription()
    {
        if (descRoot != null)
            descRoot.SetActive(false);
    }

    public void SelectRest()
    {
        int healAmount = Mathf.CeilToInt(player.maxHp * healRatio);
        int healed = player.Heal(healAmount);
        if (healed > 0)
            AudioManager.Instance?.PlaySfx(healSound); // 실제로 회복됐을 때만 회복 효과음을 재생합니다.

        HideDescription();
        HideOptions();

        if (messageText != null)
            messageText.text = $"체력을 {healed} 회복했습니다.";

        Debug.Log($"체력을 {healed} 회복했습니다.");

        if (continueButton != null)
            continueButton.SetActive(true);
    }

    public void SelectUpgrade()
    {
        for (int i = 0; i < player.masterDeck.Count; i++)
        {
            if(player.masterDeck[i].isUpgraded) continue;

            var btn = Instantiate(viewCard, btnParent).GetComponent<ViewCard>();
            btn.Init(player.masterDeck[i]);
            btn.GetComponent<RestOptionButton>().restPanelController = this;
            cards.Add(btn);
        }
        panels.SetActive(true);
    }

    public void Upgrade(ViewCard card)
    {
        Skill skill = card.skill;
        skill.isUpgraded = true;
        skill.desc = skill.effect.FormatDesc(skill, 0);

        HideDescription();
        HideOptions();
        continueButton.SetActive(true);
        panels.SetActive(false);
        for(int i = cards.Count - 1; i >= 0; i--)
        {
            Destroy(cards[i].gameObject);
            cards.Remove(cards[i]);
        }
    }

    private void ResetView()
    {
        if (restOption != null)
            restOption.SetActive(true);

        if (upgradeOption != null)
            upgradeOption.SetActive(true);

        if (continueButton != null)
            continueButton.SetActive(false);

        if (messageText != null)
            messageText.text = "";

        HideDescription();
    }

    private void ShowDescription(string description)
    {
        if (descText != null)
            descText.text = description;

        if (descRoot != null)
            descRoot.SetActive(true);
    }

    private void HideOptions()
    {
        // 휴식을 선택하면 선택지는 사라지고 Continue 버튼만 남습니다.
        if (restOption != null)
            restOption.SetActive(false);

        if (upgradeOption != null)
            upgradeOption.SetActive(false);
    }
}
