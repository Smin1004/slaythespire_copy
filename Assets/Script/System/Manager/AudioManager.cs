using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // BattleManager가 전달한 EnemyData/EnemyAction 사운드를 실제로 재생하는 SFX 채널입니다.
    [SerializeField] private AudioSource sfxSource;

    private void Awake()
    {
        if (sfxSource == null)
            sfxSource = GetComponent<AudioSource>();

        if (sfxSource == null)
            sfxSource = gameObject.AddComponent<AudioSource>();
    }

    public void PlaySfx(AudioClip clip)
    {
        // 클립이나 AudioSource가 비어 있어도 전투 흐름은 멈추지 않게 합니다.
        if (clip == null || sfxSource == null)
            return;

        sfxSource.PlayOneShot(clip);
    }
}
