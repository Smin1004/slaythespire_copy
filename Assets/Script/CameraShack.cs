using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    Vector3 _originPos;

    private void Awake()
    {
        _originPos = transform.localPosition;
    }

    private void Update()
    {
        
    }
    public void Shake(float duration, float magnitude)
    {
        StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition =
                _originPos + new Vector3(x, y, 0);

            yield return null;
        }

        transform.localPosition = _originPos;
    }
}