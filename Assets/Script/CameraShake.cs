using System;
using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [Header("Shake Settings")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float defaultDuration = 0.3f;
    [SerializeField] private float defaultMagnitude = 0.15f;

    [Header("Hit Image Settings")]
    [SerializeField] private GameObject hitImage;
    [SerializeField] private float blinkInterval = 0.1f;
    [SerializeField] private int blinkCount = 3;

    public static event Action<float, float> OnShakeRequested;

    private Vector3 originalCamPos;
    private Coroutine shakeCoroutine;
    private Coroutine blinkCoroutine;

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }
    private void OnEnable()
    {
        OnShakeRequested += PlayCameraShake;
    }

    private void OnDisable()
    {
        OnShakeRequested -= PlayCameraShake;
    }

    public static void RequestShake(float duration, float magnitude)
    {
        OnShakeRequested?.Invoke(duration, magnitude);
    }

    public void PlayDefaultShake()
    {
        PlayCameraShake(defaultDuration, defaultMagnitude);
    }

    private void Start()
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
        PlayCameraShake(defaultDuration, defaultMagnitude);

        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);

        if (hitImage != null)
            blinkCoroutine = StartCoroutine(Blink());
    }

    public void PlayCameraShake(float duration, float magnitude)
    {
        if (mainCamera == null)
            return;

        if (shakeCoroutine != null)
            StopCoroutine(shakeCoroutine);

        shakeCoroutine = StartCoroutine(Shake(duration, magnitude));
    }

    private IEnumerator Shake(float duration, float magnitude)
    {
        Vector3 originalCamPos = mainCamera.transform.localPosition;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float strength = Mathf.Lerp(magnitude, 0f, elapsed / duration);

            mainCamera.transform.localPosition = originalCamPos + new Vector3(
                UnityEngine.Random.Range(-1f, 1f) * strength,
                UnityEngine.Random.Range(-1f, 1f) * strength,
                0f
            );

            yield return null;
        }

        mainCamera.transform.localPosition = originalCamPos;
        shakeCoroutine = null;
    }

    private IEnumerator Blink()
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
