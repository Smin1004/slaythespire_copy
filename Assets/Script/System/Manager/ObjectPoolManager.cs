using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    // 어디서든 접근 가능한 싱글톤 인스턴스
    public static ObjectPoolManager Instance;

    // 프리팹(Key)과 해당 풀(Value)을 매핑하는 딕셔너리
    private Dictionary<GameObject, Pool> poolDictionary = new Dictionary<GameObject, Pool>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 풀 데이터를 관리하는 내부 클래스
    [System.Serializable]
    public class Pool
    {
        public Queue<GameObject> inactiveObjects = new Queue<GameObject>(); // 비활성화된 오브젝트 대기열
        public Transform parent; // 하이어라키 정리를 위한 부모 트랜스폼
    }

    /// <summary>
    /// 오브젝트를 풀에서 가져옵니다 (Instantiate 대체)
    /// </summary>
    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        // 1. 해당 프리팹에 대한 풀이 없으면 새로 생성
        if (!poolDictionary.ContainsKey(prefab))
        {
            CreatePool(prefab);
        }

        Pool pool = poolDictionary[prefab];
        GameObject obj;

        // 2. 대기 중인 오브젝트가 있다면 꺼내서 재사용
        if (pool.inactiveObjects.Count > 0)
        {
            obj = pool.inactiveObjects.Dequeue();
        }
        // 3. 대기 중인 오브젝트가 없다면 새로 생성 (부모 설정 포함)
        else
        {
            obj = Instantiate(prefab, pool.parent);

            // *추가된 부분: 생성된 오브젝트가 자신의 원본을 알게 함*
            var poolable = obj.GetComponent<PoolableObject>();
            if (poolable != null) poolable.SetPrefab(prefab);
        }

        // 4. 위치, 회전 설정 및 활성화
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.SetActive(true);

        return obj;
    }

    /// <summary>
    /// 오브젝트를 풀로 반환합니다 (Destroy 대체)
    /// </summary>
    public void Despawn(GameObject obj, GameObject originalPrefab)
    {
        if (!poolDictionary.ContainsKey(originalPrefab))
        {
            Debug.LogWarning($"풀에 등록되지 않은 프리팹입니다: {originalPrefab.name}. 그냥 파괴합니다.");
            Destroy(obj);
            return;
        }

        obj.SetActive(false); // 비활성화
        poolDictionary[originalPrefab].inactiveObjects.Enqueue(obj); // 큐에 반환
    }

    // 내부적으로 풀을 생성하는 함수
    private void CreatePool(GameObject prefab)
    {
        Pool newPool = new Pool();

        // 하이어라키 창 정리를 위해 빈 오브젝트 생성 (예: "Pool_EnemyBasic")
        GameObject parentObj = new GameObject("Pool_" + prefab.name);
        parentObj.transform.SetParent(this.transform); // 매니저 하위로 들어감

        newPool.parent = parentObj.transform;
        poolDictionary.Add(prefab, newPool);
    }
}