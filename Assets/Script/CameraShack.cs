using System.Collections;
using UnityEngine;

public class CameraShack : MonoBehaviour
{
    [Header("Shake Settings")]
    // 흔들릴 메인 카메라입니다. 비워두면 Camera.main을 사용합니다.
    [SerializeField] Camera mainCamera;
    // 흔들림이 지속되는 시간입니다.
    [SerializeField] float shakeDuration = 0.3f;
    // 흔들림 강도입니다.
    [SerializeField] float shakeMagnitude = 0.15f;

    [Header("Hit Image Settings")]
    // 플레이어가 맞았을 때 잠깐 켜질 피격 이미지입니다.
    [SerializeField] GameObject hitImage;
    // 피격 이미지가 깜빡이는 간격입니다.
    [SerializeField] float blinkInterval = 0.1f;
    // 피격 이미지 깜빡임 횟수입니다.
    [SerializeField] int blinkCount = 3;

    private Vector3 originalCamPos;
    private Coroutine shakeCoroutine;
    private Coroutine blinkCoroutine;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera != null)
            originalCamPos = mainCamera.transform.localPosition;

        if (hitImage != null)
            hitImage.SetActive(false);
    }

    public void PlayHitEffect()
    {
        // 플레이어 피격용: 카메라 흔들림 + 피격 이미지 깜빡임을 같이 실행합니다.
        PlayCameraShake();

        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);

        if (hitImage != null)
            blinkCoroutine = StartCoroutine(Blink());
    }

    public void PlayCameraShake()
    {
        // 적 피격용: 화면 이미지는 켜지지 않고 카메라만 흔들립니다.
        if (mainCamera == null)
            return;

        if (shakeCoroutine != null)
            StopCoroutine(shakeCoroutine);

        shakeCoroutine = StartCoroutine(Shake());
    }

    IEnumerator Shake()
    {
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;

            float strength = Mathf.Lerp(shakeMagnitude, 0f, elapsed / shakeDuration);

            mainCamera.transform.localPosition = originalCamPos + new Vector3(
                Random.Range(-1f, 1f) * strength,
                Random.Range(-1f, 1f) * strength,
                0f
            );

            yield return null;
        }

        mainCamera.transform.localPosition = originalCamPos;
    }

    IEnumerator Blink()
    {
        for (int i = 0; i < blinkCount; i++)
        {
            hitImage.SetActive(true);
            yield return new WaitForSeconds(blinkInterval);
            hitImage.SetActive(false);
            yield return new WaitForSeconds(blinkInterval);
        }
    }
}
