using System;
using UnityEngine;

public class OptionButton : MonoBehaviour
{
    public static event Action OnSettingOpen;
    public static event Action OnSettingClose;
    public static bool IsOpen { get; private set; }

    [SerializeField] GameObject _canvas;            // 옵션창 루트
    [SerializeField] GameObject _setting;           // 기본 UI
    [SerializeField] GameObject _exitbutton;        // 옵션 닫기 버튼
    [SerializeField] GameObject[] _hideWhenOpen;    // 옵션창 열 때 숨길 다른 객체들

    Canvas _canvasComponent;
    bool HasRequiredObjects => _canvas != null && _setting != null && _exitbutton != null;

    void Awake()
    {
        if (_canvas != null)
            _canvasComponent = _canvas.GetComponent<Canvas>();
    }

    void Start()
    {
        if (!HasRequiredObjects)
            return;

        _canvas.SetActive(false);
        _setting.SetActive(true);
        _exitbutton.SetActive(false);
        IsOpen = false;
    }

    void Update()
    {
        if (!HasRequiredObjects)
        {
            // If the option UI was destroyed during scene loading, only reset pause state.
            Time.timeScale = 1f;
            IsOpen = false;
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_canvas.activeSelf)
                ExitSetting();
            else
                Setting();
        }

        Time.timeScale = _canvas.activeSelf ? 0f : 1f;
    }

    public static void ResetData()
    {
        PlayerPrefs.DeleteAll();
    }

    public void Setting()
    {
        if (!HasRequiredObjects)
            return;

        if (_canvasComponent != null)
        {
            _canvasComponent.overrideSorting = true;
            _canvasComponent.sortingOrder = 1000;
        }

        _canvas.SetActive(true);
        _setting.SetActive(false);
        _exitbutton.SetActive(true);
        SetHiddenObjects(false);
        IsOpen = true;
        OnSettingOpen?.Invoke();
    }

    public void ExitSetting()
    {
        if (!HasRequiredObjects)
            return;

        _canvas.SetActive(false);
        _setting.SetActive(true);
        _exitbutton.SetActive(false);
        SetHiddenObjects(true);
        IsOpen = false;
        OnSettingClose?.Invoke();
    }

    void SetHiddenObjects(bool active)
    {
        if (_hideWhenOpen == null) return;
        foreach (var obj in _hideWhenOpen)
        {
            if (obj != null)
                obj.SetActive(active);
        }
    }

    public void Option()
    {
        if (!HasRequiredObjects)
            return;

        if (_canvas.activeSelf)
            ExitSetting();
        else
            Setting();
    }
}
