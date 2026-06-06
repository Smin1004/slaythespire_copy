using System.Collections;
using TMPro;
using UnityEngine;

public class DamageView : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private TMP_Text _damageText;
    [SerializeField] private RectTransform _rt;
    [SerializeField] private CanvasGroup _canvasGroup;

    private Coroutine _playRoutine;
    private Vector2 _originPos;

    private void Awake()
    {
        if (_rt != null)
            _originPos = _rt.anchoredPosition;
    }

    public void Play(float damage)
    {
        if (_damageText != null)
            _damageText.text = damage.ToString("N0");

        if (_rt != null)
            _rt.anchoredPosition = _originPos;

        if (_canvasGroup != null)
            _canvasGroup.alpha = 1f;

        if (_playRoutine != null)
            StopCoroutine(_playRoutine);

        _playRoutine = StartCoroutine(PlayRoutine());
    }

    public void Play(float damage, Vector2 anchoredPosition)
    {
        _originPos = anchoredPosition;
        Play(damage);
    }

    private IEnumerator PlayRoutine()
    {
        float duration = 1f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            if (_rt != null)
            {
                Vector2 pos = _originPos;
                pos.y += t * 100f;
                _rt.anchoredPosition = pos;
            }

            if (_canvasGroup != null)
                _canvasGroup.alpha = 1f - t;

            yield return null;
        }

        if (_rt != null)
            _rt.anchoredPosition = _originPos;

        if (_canvasGroup != null)
            _canvasGroup.alpha = 1f;

        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (_canvasGroup != null)
            _canvasGroup.alpha = 1f;
    }
}
