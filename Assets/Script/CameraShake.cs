using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [Header("Shake Settings")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float shakeDuration = 0.3f;
    [SerializeField] private float shakeMagnitude = 0.15f;

    [Header("Hit Image Settings")]
    [SerializeField] private GameObject hitImage;
    [SerializeField] private float blinkInterval = 0.1f;
    [SerializeField] private int blinkCount = 3;

    private Vector3 originalCamPos;
    private Coroutine shakeCoroutine;
    private Coroutine blinkCoroutine;

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
        PlayCameraShake();

        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);

        if (hitImage != null)
            blinkCoroutine = StartCoroutine(Blink());
    }

    public void PlayCameraShake()
    {
        if (mainCamera == null)
            return;

        if (shakeCoroutine != null)
            StopCoroutine(shakeCoroutine);

        shakeCoroutine = StartCoroutine(Shake());
    }

    private IEnumerator Shake()
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
