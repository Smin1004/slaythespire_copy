using UnityEngine;

public class Test: MonoBehaviour
{

    [Header("----- 컴포넌트 -----")]
    [SerializeField] DamageViewSpawner _damageSpawner;  // 대미지뷰 스포너

    //111 Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            _damageSpawner.SpawnDamageView(Random.Range(10, 999));
    
    
    }
}
