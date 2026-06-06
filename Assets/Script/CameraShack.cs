using System.Collections;
using UnityEngine;

public class CameraShack : MonoBehaviour
{

    [Header("카메라 흔들림")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float shakeDuration = 0.3f;
    [SerializeField] private float shakeMagnitude = 0.15f;

    [Header("�ǰ� �̹��� ����")]
    [SerializeField] GameObject hitImage;         // ĵ���� �̹��� ����
    [SerializeField] float blinkInterval = 0.1f;  // �����̴� �ӵ�
    [SerializeField] int blinkCount = 3;

    private Vector3 originalCamPos;
    private Coroutine shakeCoroutine;
    private Coroutine blinkCoroutine;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        originalCamPos = mainCamera.transform.localPosition;

        if (hitImage != null)
            hitImage.SetActive(false);

    }
    void Update()
    {
        // �����̽��� ������ ��鸲 ����
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PlayHitEffect();
        }
    }

    public void PlayHitEffect()
    {
        if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
        if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);

        shakeCoroutine = StartCoroutine(Shake());
        blinkCoroutine = StartCoroutine(Blink());
    }

    public void PlayAttackShake()
    {
        if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
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