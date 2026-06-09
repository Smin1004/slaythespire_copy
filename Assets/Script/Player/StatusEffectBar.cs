using System.Collections.Generic;
using UnityEngine;

public class StatusEffectBar : MonoBehaviour
{
    [SerializeField] private Entity targetEntity;
    [SerializeField] private StatusEffectIcon iconPrefab;
    [SerializeField] private RectTransform iconParent;
    [SerializeField] private Vector2 startOffset = new Vector2(0f, -28f);
    [SerializeField] private float iconSpacing = 32f;

    private readonly List<StatusEffectIcon> spawnedIcons = new();

    private void Awake()
    {
        if (targetEntity == null)
            targetEntity = GetComponentInParent<Entity>();

        if (iconParent == null)
            iconParent = transform as RectTransform;
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
        if (targetEntity != null)
            targetEntity.OnBuffsChanged -= Refresh;

        targetEntity = entity;

        if (targetEntity != null)
            targetEntity.OnBuffsChanged += Refresh;

        Refresh(targetEntity != null ? targetEntity.Buffs : null);
    }

    private void Refresh(IReadOnlyList<Buff> buffs)
    {
        ClearIcons();

        if (buffs == null || iconPrefab == null || iconParent == null)
            return;

        for (int i = 0; i < buffs.Count; i++)
        {
            StatusEffectIcon icon = Instantiate(iconPrefab, iconParent);
            icon.Setup(buffs[i]);
            spawnedIcons.Add(icon);

            if (icon.transform is RectTransform rectTransform)
                rectTransform.anchoredPosition = startOffset + new Vector2(i * iconSpacing, 0f);
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
}
