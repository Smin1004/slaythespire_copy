using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 대미지뷰를 스폰해 주는 역할
/// (오브젝트 풀링)
/// </summary>
public class DamageViewSpawner : MonoBehaviour
{
    [Header("----- 리소스 -----")]
    [SerializeField] DamageView _damageViewPrefab;  // 대미지뷰 프리팹

    [Header("----- 컴포넌트 -----")]
    [SerializeField] Transform _damageViewParent;   // 대미지뷰 부모

    [Header("----- 오브젝트 풀(읽기 전용) -----")]
    [SerializeField] List<DamageView> _damageViewPool = new();


    private void Start()
    {
        // 대미지뷰 10개를 미리 생성하여 풀에 등록해 놓기
        for(int i = 0; i < 10; i++)
        {
            CreateDamageView();
        }
    }
  


    /// <summary>
    /// 대미지뷰 복제본을 미리 생성해 풀에 등록하는 함수
    /// </summary>
    /// <returns>생성된 대미지뷰</returns>
    public DamageView CreateDamageView()
    {
        // 대미지뷰 프리팹 복제본을 씬에 생성
        DamageView damageView = Instantiate(_damageViewPrefab, _damageViewParent);

        // 복제본 비활성화
        damageView.gameObject.SetActive(false);

        // 풀에 추가
        _damageViewPool.Add(damageView);

        return damageView;
    }

    /// <summary>
    /// 풀에서 대기 중인 대미지뷰를 꺼내 반환하는 함수
    /// </summary>
    /// <returns>활성화된 대미지뷰</returns>
    public DamageView PopDamageView()
    {
        // 풀에 있는 모든 대미지뷰 방문
        foreach(var damageView in _damageViewPool)
        {
            // 이번에 확인한 대미지뷰 게임오브젝트가
            // 비활성화 상태이면
            if(damageView.gameObject.activeInHierarchy == false)
            {
                // 활성화하여 반환
                damageView.gameObject.SetActive(true);
                return damageView;
            }
        }

        // 풀에 있는 모든 대미지뷰 게임오브젝트가 활성화 상태이면
        DamageView newDamageView = CreateDamageView();
        newDamageView.gameObject.SetActive(true);
        return newDamageView;
    }

    /// <summary>
    /// 대미지뷰를 스폰하는 함수
    /// </summary>
    /// <param name="damage"></param>
    public void SpawnDamageView(float damage)
    {
        // 대미지뷰를 풀에서 가져오기
        DamageView damageView = PopDamageView();

        // 대미지뷰 설정
        damageView.Play(damage);
    }

   
}
