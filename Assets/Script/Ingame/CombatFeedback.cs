using UnityEngine;

public class CombatFeedback : MonoBehaviour
{
    [Header("피격 대상")]
    // 데미지를 받는 Entity입니다. 비워두면 같은 오브젝트의 Entity를 자동으로 찾습니다.
    [SerializeField] private Entity targetEntity;

    [Header("화면 피드백")]
    // 데미지 숫자를 대상 머리 위에 띄워주는 스포너입니다.
    [SerializeField] private DamageSpawner damageSpawner;
    // 카메라 흔들림과 피격 이미지 깜빡임을 실행하는 컴포넌트입니다.
    [SerializeField] private CameraShake cameraShack;

    [Header("사운드")]
    // 프로젝트에서 쓰는 공용 사운드 매니저입니다.
    [SerializeField] private AudioManager soundManager;
    // 사운드 매니저를 쓰지 않을 때 직접 재생할 AudioSource입니다.
    [SerializeField] private AudioSource audioSource;
    // 이 대상이 피격될 때 재생할 효과음입니다.
    [SerializeField] private AudioClip hitSfx;

    [Header("카메라 효과 방식")]
    // true면 흔들림 + 피격 이미지, false면 카메라 흔들림만 실행합니다.
    [SerializeField] private bool useFullHitEffect;

    private void Reset()
    {
        targetEntity = GetComponent<Entity>();
    }

    private void Awake()
    {
        if (targetEntity == null)
            targetEntity = GetComponent<Entity>();
    }

    private void OnEnable()
    {
        // Entity가 실제 HP 데미지를 받았을 때 피드백을 실행합니다.
        if (targetEntity != null)
            targetEntity.OnDamaged += PlayDamagedFeedback;
    }

    private void OnDisable()
    {
        if (targetEntity != null)
            targetEntity.OnDamaged -= PlayDamagedFeedback;
    }

    private void PlayDamagedFeedback(int damage)
    {
        // 피격 대상 위치 기준으로 데미지 텍스트를 띄웁니다.
        if (damageSpawner != null)
            damageSpawner.SpawnDamage(damage, transform);

        if (cameraShack != null)
        {
            if (useFullHitEffect)
                cameraShack.PlayHitEffect();
            else
                cameraShack.PlayCameraShake();
        }

        PlaySfx();
    }

    private void PlaySfx()
    {
        if (hitSfx == null)
            return;

        if (soundManager != null)
        {
            soundManager.PlaySfx(hitSfx);
            return;
        }

        if (audioSource != null)
            audioSource.PlayOneShot(hitSfx);
    }
}
