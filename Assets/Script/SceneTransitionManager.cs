using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }
    
    [SerializeField] private CanvasGroup fadeCanvasGroup; // FadeOverlay의 CanvasGroup입니다.
    [SerializeField] private float fadeDuration = 0.35f;  // 암전/해제에 걸리는 시간입니다.

    private void Awake()
    {
        // 이미 SceneTransitionManager가 있으면 중복 생성을 막습니다.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Keep only the manager alive so its loading coroutine can finish.
        DontDestroyOnLoad(gameObject);

        // 시작할 때는 화면을 투명하게 둡니다.
        FindFadeCanvasGroup();
        SetFadeAlpha(0f, false);
    }

    
    public void LoadSceneWithFade(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
            return;

        StartCoroutine(LoadSceneRoutine(sceneName));
    }

   
    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        yield return Fade(1f);

        // Reset pause state and stop the previous scene BGM before loading.
        Time.timeScale = 1f;
        AudioManager.Instance?.StopBgm();

        SceneManager.LoadScene(sceneName);

        // 새 씬 오브젝트들이 초기화될 시간을 한 프레임 줍니다.
        yield return null;

        // Use the FadeOverlay that belongs to the newly loaded scene.
        fadeCanvasGroup = null;
        FindFadeCanvasGroup();
        SetFadeAlpha(1f, true);

        yield return Fade(0f);
    }

    /// <summary>
    /// CanvasGroup alpha 값을 목표값까지 서서히 변경합니다.
    /// targetAlpha가 1이면 검은 화면, 0이면 투명 화면입니다.
    /// </summary>
    private IEnumerator Fade(float targetAlpha)
    {
        if (fadeCanvasGroup == null)
            yield break;

        fadeCanvasGroup.gameObject.SetActive(true);
        fadeCanvasGroup.blocksRaycasts = true;

        float startAlpha = fadeCanvasGroup.alpha;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            yield return null;
        }

        SetFadeAlpha(targetAlpha, targetAlpha > 0f);
    }

    /// <summary>
    /// 페이드 상태를 즉시 설정합니다.
    /// </summary>
    private void SetFadeAlpha(float alpha, bool blockInput)
    {
        if (fadeCanvasGroup == null)
            FindFadeCanvasGroup();

        if (fadeCanvasGroup == null)
            return;

        fadeCanvasGroup.alpha = alpha;
        fadeCanvasGroup.blocksRaycasts = blockInput;
        fadeCanvasGroup.interactable = blockInput;

        fadeCanvasGroup.gameObject.SetActive(alpha > 0f || blockInput);
    }

    private void FindFadeCanvasGroup()
    {
        if (fadeCanvasGroup != null)
            return;

        GameObject fadeObject = GameObject.Find("FadeOverlay");
        if (fadeObject != null)
            fadeCanvasGroup = fadeObject.GetComponent<CanvasGroup>();
    }
}
