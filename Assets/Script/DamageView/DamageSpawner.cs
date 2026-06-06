using System.Collections.Generic;
using UnityEngine;

public class DamageSpawner : MonoBehaviour
{
    [Header("데미지 텍스트")]
    // 기존 DamageViewSpawner가 있으면 그 풀을 우선 사용합니다.
    [SerializeField] private DamageViewSpawner pooledSpawner;
    // DamageViewSpawner가 없을 때만 직접 풀링에 사용할 프리팹입니다.
    [SerializeField] private DamageView damagePopupPrefab;
    // 데미지 텍스트가 생성될 UI Canvas의 RectTransform입니다.
    [SerializeField] private RectTransform canvasParent;

    [Header("좌표 변환")]
    // 월드 위치를 화면 위치로 바꿀 때 기준이 되는 카메라입니다.
    [SerializeField] private Camera worldCamera;
    // Screen Space - Camera 캔버스라면 UI 카메라를 넣습니다. Overlay면 비워도 됩니다.
    [SerializeField] private Camera uiCamera;
    // 대상 위치에서 얼마나 위에 데미지 텍스트를 띄울지 정합니다.
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2f, 0f);

    [Header("백업 풀링")]
    // DamageViewSpawner가 없을 때 미리 만들어둘 데미지 텍스트 개수입니다.
    [SerializeField] private int initialPoolSize = 10;
    // DamageViewSpawner가 없을 때 사용하는 백업 풀입니다.
    [SerializeField] private List<DamageView> damageViewPool = new();

    private void Awake()
    {
        if (pooledSpawner == null)
            pooledSpawner = GetComponent<DamageViewSpawner>();
    }

    private void Start()
    {
        // 기존 DamageViewSpawner가 있으면 그쪽에서 풀을 만들기 때문에 여기서는 추가 생성하지 않습니다.
        if (pooledSpawner != null)
            return;

        for (int i = damageViewPool.Count; i < initialPoolSize; i++)
            CreateDamageView();
    }

    public void SpawnDamage(float damage, Transform target)
    {
        if (target == null)
            return;

        SpawnDamage(damage, target.position + worldOffset);
        
    }

    public void SpawnDamage(float damage, Vector3 worldPosition)
    {
        if (canvasParent == null)
            return;

        if (worldCamera == null)
            worldCamera = Camera.main;

        // 월드 좌표를 캔버스 안의 anchoredPosition으로 변환합니다.
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(worldCamera, worldPosition);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasParent, screenPoint, uiCamera, out Vector2 anchoredPosition);

        if (pooledSpawner != null)
        {
            pooledSpawner.SpawnDamageView(damage, anchoredPosition);
            return;
        }

        if (damagePopupPrefab == null)
            return;

        DamageView damageView = PopDamageView();
        damageView.Play(damage, anchoredPosition);

        
        Debug.Log($"World : {worldPosition}");
        Debug.Log($"Screen : {screenPoint}");
        Debug.Log($"Anchored : {anchoredPosition}");
       
    }

    private DamageView PopDamageView()
    {
        foreach (DamageView damageView in damageViewPool)
        {
            if (damageView != null && !damageView.gameObject.activeInHierarchy)
            {
                damageView.gameObject.SetActive(true);
                return damageView;
            }
        }

        DamageView newDamageView = CreateDamageView();
        newDamageView.gameObject.SetActive(true);
        return newDamageView;
    }

    private DamageView CreateDamageView()
    {
        DamageView damageView = Instantiate(damagePopupPrefab, canvasParent);
        damageView.gameObject.SetActive(false);
        damageViewPool.Add(damageView);
        return damageView;
    }
}
