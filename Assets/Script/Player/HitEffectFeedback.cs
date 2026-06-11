using UnityEngine;

public class HitEffectFeedback : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private Entity entity;
 
    [Header("이펙트")]
    [SerializeField] private GameObject hitEffectPrefab;

    [Header("생성 위치")]
    [SerializeField] private Transform hitEffectPoint;

    [Header("위치 보정")]
    [SerializeField] private Vector3 positionOffset = new Vector3(0f, 0.5f, 0f);

    [Header("이펙트 유지 시간")]
    [SerializeField] private float effectLifeTime = 0.5f;

    [Header("카메라 쉐이크")]
    [SerializeField] private bool useCameraShake = true;
    [SerializeField] private float shakeDuration;
    [SerializeField] private float shakeMagnitude;


    private void Awake()
    {
        if (entity == null)
            entity = GetComponent<Entity>();

        if (hitEffectPoint == null)
            hitEffectPoint = transform;
    }

    private void OnEnable()
    {
        if (entity != null)
            entity.OnHitReceived += PlayHitEffect;
    }

    private void OnDisable()
    {
        if (entity != null)
            entity.OnHitReceived -= PlayHitEffect;
    }

    private void PlayHitEffect(int damage)
    {
        SpawnHitEffect();

        if (useCameraShake)
            CameraShake.RequestShake(shakeDuration, shakeMagnitude);
    }

    private void SpawnHitEffect()
    {
        if (hitEffectPrefab == null || hitEffectPoint == null)
            return;

        Vector3 spawnPosition = hitEffectPoint.position + positionOffset;

        GameObject effect = Instantiate(
            hitEffectPrefab,
            spawnPosition,
            Quaternion.identity
        );

        Destroy(effect, effectLifeTime);
    }
}
