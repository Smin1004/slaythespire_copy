using System.Collections.Generic;
using UnityEngine;

public class DamageViewSpawner : MonoBehaviour
{
    [Header("Resource")]
    [SerializeField] private DamageView _damageViewPrefab;

    [Header("Components")]
    [SerializeField] private Transform _damageViewParent;

    [Header("Pool")]
    [SerializeField] private List<DamageView> _damageViewPool = new();

    private void Start()
    {
        for (int i = 0; i < 10; i++)
            CreateDamageView();
    }

    public DamageView CreateDamageView()
    {
        DamageView damageView = Instantiate(_damageViewPrefab, _damageViewParent);
        damageView.gameObject.SetActive(false);
        _damageViewPool.Add(damageView);
        return damageView;
    }

    public DamageView PopDamageView()
    {
        foreach (var damageView in _damageViewPool)
        {
            if (!damageView.gameObject.activeInHierarchy)
            {
                damageView.gameObject.SetActive(true);
                return damageView;
            }
        }

        DamageView newDamageView = CreateDamageView();
        newDamageView.gameObject.SetActive(true);
        return newDamageView;
    }

    public void SpawnDamageView(float damage)
    {
        DamageView damageView = PopDamageView();
        damageView.Play(damage);
    }

    public void SpawnDamageView(float damage, Vector2 anchoredPosition)
    {
        DamageView damageView = PopDamageView();
        damageView.Play(damage, anchoredPosition);
    }
}
