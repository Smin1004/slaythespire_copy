using System.Collections.Generic;
using UnityEngine;

public class StatusEffectBar : MonoBehaviour
{
    [SerializeField] private Entity targetEntity;
    [SerializeField] private StatusEffectIcon iconPrefab;
    [SerializeField] private StatusEffectTooltip tooltip;
    [SerializeField] private RectTransform iconParent;
    [SerializeField] private Vector2 startOffset = Vector2.zero;
    [SerializeField] private Vector2 iconSize = new Vector2(48f, 48f);
    [SerializeField] private float iconSpacing = 56f;

    private readonly List<StatusEffectIcon> spawnedIcons = new();

    private void Awake()
    {
        if (targetEntity == null)
            targetEntity = GetComponentInParent<Entity>();

        if (iconParent == null)
            iconParent = transform as RectTransform;

        if (iconParent != null)
        {
            iconParent.anchorMin = new Vector2(0f, 0.5f);
            iconParent.anchorMax = new Vector2(0f, 0.5f);
            iconParent.pivot = new Vector2(0f, 0.5f);
        }
    }

    public void SetTooltip(StatusEffectTooltip assignedTooltip)
    {
        // EnemyUI처럼 상위 UI에서 찾은 설명창을 이 상태이상 바가 생성하는 아이콘들에게 전달합니다.
        tooltip = assignedTooltip;
    }

    private void OnEnable()
    {
        if (targetEntity != null)
            targetEntity.OnBuffsChanged += Refresh;
    }

    private void Start()
    {
        if (targetEntity != null)
            Refresh(targetEntity.Buffs);
    }

    private void OnDisable()
    {
        if (targetEntity != null)
            targetEntity.OnBuffsChanged -= Refresh;
    }

    public void SetTarget(Entity entity)
    {
        // UI 프리팹을 재사용할 수 있으므로 기존 Entity 이벤트 연결을 먼저 끊습니다.
        if (targetEntity != null)
            targetEntity.OnBuffsChanged -= Refresh;

        targetEntity = entity;

        // 새로 연결된 Entity의 버프/디버프가 바뀔 때마다 아이콘을 다시 그립니다.
        if (targetEntity != null)
            targetEntity.OnBuffsChanged += Refresh;

        // 바인딩 직후 현재 상태도 바로 표시합니다.
        Refresh(targetEntity != null ? targetEntity.Buffs : null);
    }

    private void Refresh(IReadOnlyList<Buff> buffs)
    {
        ClearIcons();

        if (buffs == null || iconParent == null)
            return;

        for (int i = 0; i < buffs.Count; i++)
        {
            StatusEffectIcon icon = iconPrefab != null
                ? Instantiate(iconPrefab, iconParent)
                : CreateRuntimeIcon(iconParent);

            icon.Setup(buffs[i], tooltip);
            spawnedIcons.Add(icon);

            if (icon.transform is RectTransform rectTransform)
            {
                rectTransform.sizeDelta = iconSize;
                rectTransform.anchoredPosition = startOffset + new Vector2(i * iconSpacing, 0f);
            }
        }
    }

    private void ClearIcons()
    {
        foreach (StatusEffectIcon icon in spawnedIcons)
        {
            if (icon != null)
                Destroy(icon.gameObject);
        }

        spawnedIcons.Clear();
    }

    private StatusEffectIcon CreateRuntimeIcon(RectTransform parent)
    {
        GameObject iconObject = new GameObject("StatusEffectIcon", typeof(RectTransform), typeof(StatusEffectIcon));
        iconObject.transform.SetParent(parent, false);

        RectTransform rectTransform = iconObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = iconSize;

        return iconObject.GetComponent<StatusEffectIcon>();
    }
}
