using System.Collections;
using TMPro;
using UnityEngine;

public class DamageView : MonoBehaviour
{
    [Header("컴포넌트")]
    [SerializeField] private TMP_Text _damageText;
    [SerializeField] private RectTransform _rt;
    [SerializeField] private CanvasGroup _canvasGroup;

    private Coroutine _playRoutine;
    private Vector2 _originPos;
    private void Awake()
    {
        _originPos = _rt.anchoredPosition;
    }
    public void Play(float damage)
    {
        _damageText.text = damage.ToString("N0");

        _rt.anchoredPosition = _originPos;
        _canvasGroup.alpha = 1f;

        if (_playRoutine != null)
            StopCoroutine(_playRoutine);

        _playRoutine = StartCoroutine(PlayRoutine());
    }

    IEnumerator PlayRoutine()
    {
        // 애니메이션 총 재생 시간(1초)
        float duration = 1f;

        // 현재까지 경과한 시간
        float elapsedTime = 0;

        // duration 시간 동안 반복
        while (elapsedTime < duration)
        {
            // 경과 시간 누적
            elapsedTime += Time.deltaTime;

            // 0 ~ 1 사이의 진행률 계산
            // 0 = 시작
            // 1 = 종료
            float t = elapsedTime / duration;

            // 원래 위치를 기준으로 위치 계산
            Vector2 pos = _originPos;

            // t가 증가할수록 위로 100만큼 이동
            pos.y += t * 100f;

            // 실제 위치 적용
            _rt.anchoredPosition = pos;

            // 알파값 감소
            // 시작 : 1
            // 종료 : 0
            _canvasGroup.alpha = 1f - t;

            // 다음 프레임까지 대기
            yield return null;
        }

        // 혹시 오차가 있을 수 있으므로
        // 원래 위치로 강제 복구
        _rt.anchoredPosition = _originPos;

        // 알파값도 원래대로 복구
        _canvasGroup.alpha = 1f;

        // 오브젝트 풀링을 위해 비활성화
        // Destroy 대신 SetActive(false) 사용
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (_canvasGroup != null)
            _canvasGroup.alpha = 1;
    }
}