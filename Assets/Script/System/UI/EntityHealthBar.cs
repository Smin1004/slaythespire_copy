using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EntityHealthBar : MonoBehaviour
{
    [SerializeField] private Entity targetEntity;
    [SerializeField] private Image fillImage;
    [SerializeField] private TMP_Text hpText;

    private void Awake()
    {
        if (targetEntity == null)
            targetEntity = GetComponentInParent<Entity>();
    }

    private void OnEnable()
    {
        if (targetEntity == null)
        {
            Debug.LogWarning("EntityHealthBar: targetEntity가 설정되어 있지 않습니다.");
            return;
        }

        targetEntity.OnHealthChanged += OnHealthChanged;
        OnHealthChanged(targetEntity.CurrentHp, targetEntity.MaxHp);
    }

    private void OnDisable()
    {
        if (targetEntity != null)
            targetEntity.OnHealthChanged -= OnHealthChanged;
    }

    private void OnHealthChanged(int currentHp, int maxHp)
    {
        if (fillImage != null)
            fillImage.fillAmount = maxHp > 0 ? (float)currentHp / maxHp : 0f;

        if (hpText != null)
            hpText.text = $"{currentHp}/{maxHp}";
    }
}
