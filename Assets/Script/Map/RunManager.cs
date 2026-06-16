using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// 한 판의 진행 흐름을 관리하는 스크립트입니다.
/// 
/// 붙이는 위치:
/// - Managers 오브젝트
/// - 또는 RunManager 전용 오브젝트
/// 
/// 역할:
/// - 현재 진행도 관리
/// - 랜덤 선택지 2~3개 생성
/// - 10스택 도달 시 보스 선택지 표시
/// - 선택한 노드에 따라 전투/이벤트/휴식 실행
/// 
/// 다른 스크립트와의 관계:
/// - TimingController가 InitAwake(), InitStart()를 호출합니다.
/// - MapSelectView.Show()를 호출해서 선택지 UI를 띄웁니다.
/// - BattleManager.StartNormalBattle(), StartBossBattle()를 호출합니다.
/// - RewardManager가 보상 선택 완료 후 CompleteBattleReward()를 호출해야 합니다.
/// - EventPanel/RestPanel의 Continue 버튼이 CompleteNonBattleNode()를 호출해야 합니다.
/// </summary>
public class RunManager : MonoBehaviour
{
    private static RunManager _instance;
    public static RunManager Instance => _instance;

    [Header("진행도")]
    [SerializeField] private int currentDepth = 0; // 현재 진행도
    [SerializeField] private int bossDepth = 10;   // 이 수치에 도달하면 보스 노드 등장

    [Header("선택지 개수")]
    [SerializeField] private int minChoiceCount = 2;
    [SerializeField] private int maxChoiceCount = 3;

    [Header("매니저 참조")]
    [SerializeField] private BattleManager battleManager;
    [SerializeField] private MapSelectView mapSelectView;

    [Header("화면 Root")]
    [SerializeField] private GameObject battleRoot;      // 기존 전투 Root입니다. 아래 두 Root가 비어 있으면 fallback으로 사용합니다.
    [SerializeField] private GameObject battleWorldRoot; // 플레이어, 적, 스폰 포인트처럼 월드에 있어야 하는 오브젝트 묶음입니다.
    [SerializeField] private GameObject battleUIRoot;    // 카드, HP, 의도 아이콘처럼 Canvas 안에 있어야 하는 UI 묶음입니다.
    [SerializeField] private GameObject rewardPanel; // 보상 패널
    [SerializeField] private GameObject eventPanel;  // 이벤트 패널
    [SerializeField] private GameObject restPanel;   // 휴식 패널
    [SerializeField] private GameObject clearPanel;  // 클리어 패널

    [Header("Clear Result")]
    [SerializeField] private TMP_Text clearUsedCardText;
    [SerializeField] private TMP_Text clearTimeText;
    [SerializeField] private Button clearTitleButton;

    [Header("배경")]
    [SerializeField] private Image mainBackgroundImage;          // 현재 화면의 배경을 보여줄 UI Image입니다.
    [SerializeField] private Canvas backgroundCanvas;            // 배경 전용 Canvas입니다. HUD Canvas와 분리해서 플레이어를 덮지 않게 합니다.
    [SerializeField] private int backgroundSortingOrder = -100;  // 플레이어/적보다 뒤에 그려지도록 낮은 정렬값을 사용합니다.
    [SerializeField] private Vector2 backgroundSizeMultiplier = Vector2.one; // 배경 Image만 추가 확대/축소합니다. 카드 UI에는 영향을 주지 않습니다.

    [Header("맵 이동 연출")]
    [SerializeField] private CanvasGroup fadeOverlay; // 맵 이동 때 화면을 검게 덮는 CanvasGroup입니다.
    [SerializeField] private float fadeOutDuration = 0.3f;
    [SerializeField] private float fadeInDuration = 0.4f;
    [SerializeField] private float rewardOpenDelay = 0.8f; // 전투 승리 후 보상창이 바로 뜨지 않도록 잠깐 기다립니다.
    [SerializeField] private AudioClip rewardOpenSound; // 보상창이 열릴 때 재생할 효과음입니다.

    [Header("화면별 BGM")]
    [SerializeField] private AudioClip mapBgm;
    [SerializeField] private AudioClip battleBgm;
    [SerializeField] private AudioClip eventBgm;
    [SerializeField] private AudioClip restBgm;
    [SerializeField] private AudioClip bossBgm;
    [SerializeField] private AudioClip clearBgm;

    private bool isTransitioning;
    private int usedCardCount;
    private float runStartTime;

    public int CurrentDepth => currentDepth;
    public int BossDepth => bossDepth;

    /// <summary>
    /// TimingController.Awake()에서 호출됩니다.
    /// 
    /// 기존 프로젝트 구조가 Awake() 대신 InitAwake()를 사용하고 있으므로
    /// 여기서 싱글톤 Instance를 설정합니다.
    /// </summary>
    public void InitAwake()
    {
        _instance = this;
    }

    /// <summary>
    /// TimingController.Start()에서 호출됩니다.
    /// 
    /// 게임 시작 시 바로 전투를 시작하지 않고,
    /// 먼저 맵 선택지를 보여줍니다.
    /// </summary>
    public void InitStart()
    {
        currentDepth = 0;
        usedCardCount = 0;
        runStartTime = Time.realtimeSinceStartup;

        HideAllPanels();
        SetFadeAlpha(0f);
        ShowNextChoices();
    }

    public void RecordCardUse()
    {
        // 클리어 화면에 표시할 실제 카드 사용 횟수를 누적합니다.
        usedCardCount++;
    }

    /// <summary>
    /// 모든 주요 패널을 숨깁니다.
    /// 
    /// 사용 위치:
    /// - 맵 선택지를 열기 전
    /// - 전투/이벤트/휴식으로 넘어가기 전
    /// - 클리어 패널을 열기 전
    /// </summary>
    private void HideAllPanels()
    {
        SetBattleRootActive(false);

        if (rewardPanel != null)
            rewardPanel.SetActive(false);

        if (eventPanel != null)
            eventPanel.SetActive(false);

        if (restPanel != null)
            restPanel.SetActive(false);

        if (clearPanel != null)
            clearPanel.SetActive(false);

        if (mapSelectView != null)
            mapSelectView.Hide();
    }

    /// <summary>
    /// 다음 맵 선택지를 표시합니다.
    /// 
    /// 호출 위치:
    /// - 게임 시작 시 InitStart()
    /// - 전투 보상 선택 완료 후 CompleteBattleReward()
    /// - 이벤트/휴식 완료 후 CompleteNonBattleNode()
    /// </summary>
    public void ShowNextChoices()
    {
        HideAllPanels();
        // 맵 선택 화면에 들어올 때 맵 전용 BGM을 재생합니다.
        AudioManager.Instance?.PlayBgm(mapBgm);

        List<MapNodeType> choices = GenerateChoices();

        if (mapSelectView != null)
        {
            // 여기서 SelectNode를 콜백으로 넘깁니다.
            // 버튼을 누르면 MapChoiceButton → MapSelectView → RunManager.SelectNode() 순서로 실행됩니다.
            mapSelectView.Show(
                choices,
                currentDepth,
                bossDepth,
                SelectNode
            );

            ApplyBackgroundFrom(mapSelectView.gameObject);
        }
    }

    /// <summary>
    /// 전투 승리 후 보상 화면을 엽니다.
    /// BattleRoot는 유지해서 전투 결과 화면 위에 보상 UI만 띄웁니다.
    /// </summary>
    public void OpenBattleReward()
    {
        StartCoroutine(OpenBattleRewardRoutine());
    }

    private IEnumerator OpenBattleRewardRoutine()
    {
        if (mapSelectView != null)
            mapSelectView.Hide();

        if (rewardPanel != null)
            rewardPanel.SetActive(false);

        if (eventPanel != null)
            eventPanel.SetActive(false);

        if (restPanel != null)
            restPanel.SetActive(false);

        if (clearPanel != null)
            clearPanel.SetActive(false);

        SetBattleRootActive(true);

        // 승리 직후 바로 보상창을 띄우지 않고 약간의 여운을 둡니다.
        yield return new WaitForSeconds(rewardOpenDelay);

        AudioManager.Instance?.PlaySfx(rewardOpenSound);

        if (rewardPanel != null)
            rewardPanel.SetActive(true);
    }

    private void ApplyBackgroundFrom(GameObject activeRoot)
    {
        if (activeRoot == null)
            return;

        EnsureBackgroundImage();

        if (mainBackgroundImage == null)
            return;

        BackgroundProvider provider = activeRoot.GetComponentInChildren<BackgroundProvider>(true);
        Sprite backgroundSprite = provider != null ? provider.BackgroundSprite : FindBackgroundSprite(activeRoot);

        if (backgroundSprite == null)
            return;

        // 화면마다 지정한 Sprite를 하나의 UI 배경 Image에 갈아끼웁니다.
        mainBackgroundImage.sprite = backgroundSprite;
        mainBackgroundImage.enabled = true;
        mainBackgroundImage.preserveAspect = false;
        ApplyBackgroundSize();
    }

    public void ApplyGameOverBackground(GameObject gameOverRoot)
    {
        // Game over uses its own BackgroundProvider sprite for the shared background.
        ApplyBackgroundFrom(gameOverRoot);
    }

    private void EnsureBackgroundImage()
    {
        if (backgroundCanvas == null)
            backgroundCanvas = CreateBackgroundCanvas();

        SetupBackgroundCanvas(backgroundCanvas);

        if (mainBackgroundImage != null)
        {
            MoveImageToBackgroundCanvas(mainBackgroundImage);
            return;
        }

        GameObject imageObject = new GameObject("MainBackgroundImage");
        imageObject.transform.SetParent(backgroundCanvas.transform, false);

        mainBackgroundImage = imageObject.AddComponent<Image>();
        mainBackgroundImage.raycastTarget = false;
        mainBackgroundImage.preserveAspect = false;

        StretchToParent(mainBackgroundImage.rectTransform);
        ApplyBackgroundSize();
    }

    private Canvas CreateBackgroundCanvas()
    {
        GameObject canvasObject = new GameObject("MainBackgroundCanvas");
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0f;

        return canvas;
    }

    private void MoveImageToBackgroundCanvas(Image image)
    {
        if (image == null || backgroundCanvas == null)
            return;

        // 인스펙터에서 기존 Image를 넣어도, 배경 전용 Canvas 아래로 옮겨 월드 캐릭터를 덮지 않게 합니다.
        if (image.canvas != backgroundCanvas)
            image.transform.SetParent(backgroundCanvas.transform, false);

        image.raycastTarget = false;
        StretchToParent(image.rectTransform);
        ApplyBackgroundSize();
    }

    private void StretchToParent(RectTransform rectTransform)
    {
        if (rectTransform == null)
            return;

        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }

    private void ApplyBackgroundSize()
    {
        if (mainBackgroundImage == null)
            return;

        // Canvas나 카드가 아니라 배경 Image RectTransform만 키웁니다.
        RectTransform rectTransform = mainBackgroundImage.rectTransform;
        float widthMultiplier = Mathf.Max(0.01f, backgroundSizeMultiplier.x);
        float heightMultiplier = Mathf.Max(0.01f, backgroundSizeMultiplier.y);
        rectTransform.localScale = new Vector3(widthMultiplier, heightMultiplier, 1f);
    }

    private void SetupBackgroundCanvas(Canvas canvas)
    {
        if (canvas == null)
            return;

        backgroundCanvas = canvas;

        // Overlay Canvas는 월드 캐릭터 위에 무조건 그려지므로, 배경만 Screen Space - Camera Canvas로 분리합니다.
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = Camera.main;
        canvas.planeDistance = 100f;
        canvas.overrideSorting = true;
        canvas.sortingOrder = backgroundSortingOrder;
    }

    private Sprite FindBackgroundSprite(GameObject activeRoot)
    {
        Image[] images = activeRoot.GetComponentsInChildren<Image>(true);
        foreach (Image image in images)
        {
            if (image != null && image.gameObject.name == "Background")
                return image.sprite;
        }

        return null;
    }

    /// <summary>
    /// 현재 진행도에 따라 선택지를 생성합니다.
    /// 
    /// 규칙:
    /// - currentDepth가 bossDepth 이상이면 보스 선택지만 생성
    /// - 그 외에는 일반 전투/이벤트/휴식/엘리트 중 랜덤 생성
    /// </summary>
    private List<MapNodeType> GenerateChoices()
    {
        List<MapNodeType> choices = new();

        // 보스 조건 도달 시 보스 노드만 보여줍니다.
        if (currentDepth >= bossDepth)
        {
            choices.Add(MapNodeType.Boss);
            return choices;
        }

        int choiceCount = Random.Range(minChoiceCount, maxChoiceCount + 1);

        // 선택지 후보 풀입니다.
        // Battle을 2번 넣으면 일반 전투가 더 자주 등장합니다.
        List<MapNodeType> pool = new()
        {
            MapNodeType.Battle,
            MapNodeType.Battle,
            MapNodeType.Event,
            MapNodeType.Rest
        };

        // 진행도 3 이상부터 엘리트가 나올 수 있게 합니다.
        // if (currentDepth >= 3)
        //     pool.Add(MapNodeType.Elite);

        int safety = 0;

        while (choices.Count < choiceCount && safety < 50)
        {
            safety++;

            MapNodeType picked = pool[Random.Range(0, pool.Count)];

            // 같은 선택지가 중복으로 나오지 않게 합니다.
            // 예: [전투, 전투, 이벤트] 방지
            if (choices.Contains(picked))
                continue;

            choices.Add(picked);
        }

        // 혹시 중복 제거 때문에 선택지가 부족하면 일반 전투로 채웁니다.
        while (choices.Count < choiceCount)
        {
            choices.Add(MapNodeType.Battle);
        }

        return choices;
    }

    /// <summary>
    /// 맵 선택지 버튼을 눌렀을 때 호출되는 함수입니다.
    /// 
    /// 호출 흐름:
    /// MapChoiceButton 클릭
    /// → MapChoiceButton의 Button.onClick
    /// → MapSelectView에서 전달받은 onSelect 실행
    /// → RunManager.SelectNode(type) 실행
    /// </summary>
    private void SelectNode(MapNodeType type)
    {
        if (isTransitioning)
            return;

        StartCoroutine(TransitionToNode(type));
    }

    private IEnumerator TransitionToNode(MapNodeType type)
    {
        isTransitioning = true;

        // 맵 선택 후 먼저 암전시켜 화면 전환이 갑자기 튀지 않게 합니다.
        yield return FadeTo(1f, fadeOutDuration);

        HideAllPanels();

        // 보스는 최종 노드이므로 진행도를 더 올리지 않습니다.
        if (type != MapNodeType.Boss)
            currentDepth++;

        switch (type)
        {
            case MapNodeType.Battle:
                StartNormalBattle();
                break;

            // case MapNodeType.Elite:
            //     StartEliteBattle();
            //     break;

            case MapNodeType.Event:
                OpenEvent();
                break;

            case MapNodeType.Rest:
                OpenRest();
                break;

            case MapNodeType.Boss:
                StartBossBattle();
                break;
        }

        // 새 화면과 배경을 준비한 뒤 다시 밝아지게 합니다.
        yield return FadeTo(0f, fadeInDuration);

        isTransitioning = false;
    }

    /// <summary>
    /// 일반 전투 시작.
    /// BattleManager에게 전투 시작을 요청합니다.
    /// </summary>
    private void StartNormalBattle()
    {
        // 일반 전투 화면에 들어올 때 전투 BGM을 재생합니다.
        AudioManager.Instance?.PlayBgm(battleBgm);

        SetBattleRootActive(true);
        ApplyBackgroundFrom(GetBattleBackgroundRoot());

        if (battleManager != null)
            battleManager.StartNormalBattle();
    }

    /// <summary>
    /// 엘리트 전투 시작.
    /// 지금은 임시로 일반 전투와 같은 방식으로 처리합니다.
    /// 나중에 BattleManager에 StartEliteBattle()을 따로 만들면 여기서 바꾸면 됩니다.
    /// </summary>
    private void StartEliteBattle()
    {
        SetBattleRootActive(true);
        ApplyBackgroundFrom(GetBattleBackgroundRoot());

        if (battleManager != null)
            battleManager.StartNormalBattle();
    }

    /// <summary>
    /// 보스 전투 시작.
    /// BattleManager의 isBossBattle 값을 true로 바꾸고 보스 데이터를 사용하게 합니다.
    /// </summary>
    private void StartBossBattle()
    {
        // 보스 전투는 별도 BGM을 사용할 수 있게 분리합니다.
        AudioManager.Instance?.PlayBgm(bossBgm);

        SetBattleRootActive(true);
        ApplyBackgroundFrom(GetBattleBackgroundRoot());

        if (battleManager != null)
            battleManager.StartBossBattle();
    }

    /// <summary>
    /// 이벤트 패널 열기.
    /// 이벤트 패널의 완료 버튼에서 CompleteNonBattleNode()를 호출해야 합니다.
    /// </summary>
    private void OpenEvent()
    {
        // 이벤트 화면에 들어올 때 이벤트 BGM을 재생합니다.
        AudioManager.Instance?.PlayBgm(eventBgm);

        if (eventPanel != null)
        {
            eventPanel.SetActive(true);
            eventPanel.GetComponent<EventRoomManager>().Init();
            ApplyBackgroundFrom(eventPanel);
        }
    }

    /// <summary>
    /// 휴식 패널 열기.
    /// 휴식 패널의 완료 버튼에서 CompleteNonBattleNode()를 호출해야 합니다.
    /// </summary>
    private void OpenRest()
    {
        // 휴식 화면에 들어올 때 휴식 BGM을 재생합니다.
        AudioManager.Instance?.PlayBgm(restBgm);

        if (restPanel != null)
        {
            restPanel.SetActive(true);
            ApplyBackgroundFrom(restPanel);
        }
    }

    /// <summary>
    /// 이벤트/휴식 같은 비전투 노드를 완료했을 때 호출합니다.
    /// 
    /// 연결 위치:
    /// - EventPanel의 Continue 버튼 OnClick
    /// - RestPanel의 Continue 버튼 OnClick
    /// </summary>
    public void CompleteNonBattleNode()
    {
        StartCoroutine(ReturnToMapRoutine());
    }

    /// <summary>
    /// 전투 보상 선택이 끝났을 때 호출합니다.
    /// 
    /// 호출 위치:
    /// - RewardManager에서 보상 선택 완료 시
    /// - 보상 스킵 버튼을 눌렀을 때
    /// </summary>
    public void CompleteBattleReward()
    {
        StartCoroutine(ReturnToMapRoutine());
    }

    private IEnumerator ReturnToMapRoutine()
    {
        if (isTransitioning)
            yield break;

        isTransitioning = true;

        // 보상/이벤트/휴식에서 맵 선택 화면으로 돌아갈 때도 같은 fadeOverlay를 사용합니다.
        yield return FadeTo(1f, fadeOutDuration);

        // 암전 중에 남은 전투 카드 오브젝트를 제거해서 다음 전투는 새 카드로 시작합니다.
        DeckManager.Instance?.ClearBattleCards();

        ShowNextChoices();

        yield return FadeTo(0f, fadeInDuration);

        isTransitioning = false;
    }

    /// <summary>
    /// 보스 전투를 클리어했을 때 호출합니다.
    /// 
    /// 호출 위치:
    /// - BattleManager.ProcessBattleVictory()
    /// - 단, 현재 전투가 보스 전투일 때만
    /// </summary>
    public void CompleteBossBattle()
    {
        HideAllPanels();
        DeckManager.Instance?.ClearBattleCards(); // 클리어 화면으로 넘어갈 때 남아있는 손패 카드를 정리합니다.
        SetBattleRootActive(false);
        UpdateClearResultText();
        BindClearTitleButton();

        if (clearBgm != null)
            AudioManager.Instance?.PlayBgm(clearBgm);
        else
            AudioManager.Instance?.StopBgm();

        if (clearPanel != null)
        {
            clearPanel.SetActive(true);
            ApplyBackgroundFrom(clearPanel);
        }
    }

    public void ReturnToTitle()
    {
        Time.timeScale = 1f;
        AudioManager.Instance?.StopBgm(); // 타이틀로 돌아가기 전에 클리어 BGM을 정리합니다.

        // 클리어 화면 버튼에서 타이틀 씬으로 돌아갑니다.
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.LoadSceneWithFade("Title");
        else
            SceneManager.LoadScene("Title");
    }

    private void UpdateClearResultText()
    {
        BindClearResultTexts();

        if (clearUsedCardText != null)
            clearUsedCardText.text = $"사용한 카드 수: {usedCardCount}";

        if (clearTimeText != null)
            clearTimeText.text = $"클리어 시간: {FormatRunTime(Time.realtimeSinceStartup - runStartTime)}";
    }

    private void BindClearResultTexts()
    {
        if (clearPanel == null || (clearUsedCardText != null && clearTimeText != null))
            return;

        TMP_Text[] texts = clearPanel.GetComponentsInChildren<TMP_Text>(true);
        foreach (TMP_Text text in texts)
        {
            if (text == null)
                continue;

            string lowerName = text.gameObject.name.ToLowerInvariant();
            if (clearUsedCardText == null && (lowerName.Contains("card") || lowerName.Contains("use")))
                clearUsedCardText = text;
            else if (clearTimeText == null && lowerName.Contains("time"))
                clearTimeText = text;
        }

        if (clearUsedCardText == null)
            clearUsedCardText = CreateClearResultText("UsedCardText", new Vector2(0f, 40f));

        if (clearTimeText == null)
            clearTimeText = CreateClearResultText("ClearTimeText", new Vector2(0f, -20f));
    }

    private string FormatRunTime(float seconds)
    {
        // 초 단위 진행 시간을 분:초 형태로 보여줍니다.
        int totalSeconds = Mathf.Max(0, Mathf.FloorToInt(seconds));
        int minutes = totalSeconds / 60;
        int remainSeconds = totalSeconds % 60;
        return $"{minutes:00}:{remainSeconds:00}";
    }

    private TMP_Text CreateClearResultText(string objectName, Vector2 anchoredPosition)
    {
        if (clearPanel == null)
            return null;

        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(clearPanel.transform, false);

        RectTransform rectTransform = textObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = new Vector2(500f, 60f);
        rectTransform.anchoredPosition = anchoredPosition;

        TMP_Text text = textObject.GetComponent<TMP_Text>();
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 32f;
        text.color = Color.white;

        return text;
    }

    private void BindClearTitleButton()
    {
        if (clearPanel == null)
            return;

        if (clearTitleButton == null)
            clearTitleButton = clearPanel.GetComponentInChildren<Button>(true);

        if (clearTitleButton == null)
            clearTitleButton = CreateClearTitleButton();

        SetClearTitleButtonLabel(clearTitleButton);
        clearTitleButton.onClick.RemoveListener(ReturnToTitle);
        clearTitleButton.onClick.AddListener(ReturnToTitle);
    }

    private void SetClearTitleButtonLabel(Button button)
    {
        if (button == null)
            return;

        TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
        if (label != null)
            label.text = "타이틀로"; // 기존 클리어 버튼을 재사용해도 역할이 보이도록 문구를 맞춥니다.
    }

    private Button CreateClearTitleButton()
    {
        if (clearPanel == null)
            return null;

        GameObject buttonObject = new GameObject("TitleButton", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(clearPanel.transform, false);

        RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = new Vector2(260f, 70f);
        rectTransform.anchoredPosition = new Vector2(0f, -120f);

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.12f, 0.12f, 0.12f, 0.9f);

        GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(buttonObject.transform, false);

        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        TMP_Text label = labelObject.GetComponent<TMP_Text>();
        label.text = "타이틀로";
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = 30f;
        label.color = Color.white;

        return buttonObject.GetComponent<Button>();
    }

    private void SetBattleRootActive(bool active)
    {
        // 월드 오브젝트와 UI 오브젝트를 분리한 경우 둘 다 같이 켜고 끕니다.
        bool hasSplitRoots = battleWorldRoot != null || battleUIRoot != null;

        if (battleWorldRoot != null)
            battleWorldRoot.SetActive(active);

        if (battleUIRoot != null)
            battleUIRoot.SetActive(active);

        // battleRoot에는 BackgroundProvider가 붙을 수 있으므로 split root를 쓰더라도 함께 켜고 끕니다.
        if (battleRoot != null)
            battleRoot.SetActive(active);
    }

    private GameObject GetBattleBackgroundRoot()
    {
        // 전투 배경 Sprite는 전체 전투 루트의 BackgroundProvider에서 먼저 찾습니다.
        if (battleRoot != null)
            return battleRoot;

        if (battleUIRoot != null)
            return battleUIRoot;

        return battleWorldRoot;
    }

    private IEnumerator FadeTo(float targetAlpha, float duration)
    {
        if (fadeOverlay == null)
            yield break;

        fadeOverlay.gameObject.SetActive(true);
        fadeOverlay.blocksRaycasts = true;

        float startAlpha = fadeOverlay.alpha;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = duration > 0f ? elapsed / duration : 1f;
            fadeOverlay.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        SetFadeAlpha(targetAlpha);
    }

    private void SetFadeAlpha(float alpha)
    {
        if (fadeOverlay == null)
            return;

        fadeOverlay.alpha = alpha;
        fadeOverlay.blocksRaycasts = alpha > 0f;
        fadeOverlay.interactable = false;

        if (Mathf.Approximately(alpha, 0f))
            fadeOverlay.gameObject.SetActive(false);
        else
            fadeOverlay.gameObject.SetActive(true);
    }
}
