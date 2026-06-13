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

    private const string RestDescription = "현재 체력의 30%를 회복합니다.";
    private const string UpgradeDescription = "카드 한 장을 강화합니다.";

    private void OnEnable()
    {
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
        Player player = Player.Instance;
        if (player == null)
            return;

        int healAmount = Mathf.CeilToInt(player.curHp * healRatio);
        int healed = player.Heal(healAmount);

        HideDescription();
        HideOptions();

        if (messageText != null)
            messageText.text = $"체력을 {healed} 회복했습니다.";

        if (continueButton != null)
            continueButton.SetActive(true);
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
