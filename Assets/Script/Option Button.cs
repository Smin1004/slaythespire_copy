using UnityEngine;

public class Button : MonoBehaviour
{
    [SerializeField] GameObject _canvas;            // 캔버스
    [SerializeField] GameObject _setting;           // 셋팅 오브젝트
    [SerializeField] GameObject _exitbutton;        // 셋팅 x표


    void Start()
    {
        _canvas.SetActive(false); // 시작 시 Canvas 비활성화
        _setting.SetActive(true); // 시작 시 Setting 활성화
        _exitbutton.SetActive(false); // 시작 시 button 비활성화
     
    }
    void Update()
    {
        if (_canvas.activeSelf)
        {
            Time.timeScale = 0f; // 게임 일시정지
        }
        else
        {
            Time.timeScale = 1f; // 게임 재개
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Canvas 토글 (활성/비활성 전환)
            _canvas.SetActive(true);
            _setting.SetActive(false); // Setting은 Canvas와 반대로
            _exitbutton.SetActive(true);
        }
    }
    public static void ResetData()
    {
        PlayerPrefs.DeleteAll();
    }

    public void Setting()
    {
        _canvas.SetActive(true); // 시작 시 Canvas 활성화
        _setting.SetActive(false); // 시작 시 Setting 비활성화
        _exitbutton.SetActive(true); // 시작 시 button 활성화
    }

    public void ExitSetting()
    {
        _canvas.SetActive(false); // 시작 시 Canvas 활성화
        _setting.SetActive(true); // 시작 시 Setting 비활성화
        _exitbutton.SetActive(false); // 시작 시 button 활성화
    }
}
