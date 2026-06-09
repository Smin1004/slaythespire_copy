using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HpBar : MonoBehaviour
{
    [Header("연동 대상")]
    // HP를 표시할 Player 또는 Enemy의 Entity입니다.
    [SerializeField] private Entity targetEntity;

    [Header("UI")]
    // HP 양에 맞춰 fillAmount가 바뀌는 이미지입니다.
    [SerializeField] private Image fillImage;
    // "현재 HP / 최대 HP"를 표시할 텍스트입니다.
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private StatusEffectBar statusEffectBar;

    private void Awake()
    {
        if (statusEffectBar == null)
            statusEffectBar = GetComponentInChildren<StatusEffectBar>();
    }

    private void OnEnable()
    {
        // Entity의 HP가 바뀔 때마다 HP바를 갱신합니다.
        if (targetEntity != null)
            targetEntity.OnHealthChanged += UpdateHp;
    }

    private void Start()
    {
        if (targetEntity != null)
            UpdateHp(targetEntity.CurrentHp, targetEntity.MaxHp);

        if (statusEffectBar != null)
            statusEffectBar.SetTarget(targetEntity);
    }

    private void OnDisable()
    {
        if (targetEntity != null)
            targetEntity.OnHealthChanged -= UpdateHp;
    }

    public void SetTarget(Entity entity)
    {
        // 전투 중 대상이 바뀌는 경우 기존 이벤트 연결을 먼저 해제합니다.
        if (targetEntity != null)
            targetEntity.OnHealthChanged -= UpdateHp;

        targetEntity = entity;

        if (targetEntity != null)
        {
            targetEntity.OnHealthChanged += UpdateHp;
            UpdateHp(targetEntity.CurrentHp, targetEntity.MaxHp);
        }

        if (statusEffectBar != null)
            statusEffectBar.SetTarget(targetEntity);
    }

    private void UpdateHp(int currentHp, int maxHp)
    {
        float fillAmount = maxHp > 0 ? (float)currentHp / maxHp : 0f;

        if (fillImage != null)
            fillImage.fillAmount = fillAmount;

        if (hpText != null)
            hpText.text = $"{currentHp} / {maxHp}";
    }

}
