using UnityEngine;

public class Button : MonoBehaviour
{
    [SerializeField] GameObject _canvas;            // ĵ����
    [SerializeField] GameObject _setting;           // ���� ������Ʈ
    [SerializeField] GameObject _exitbutton;        // ���� xǥ


    void Start()
    {
        _canvas.SetActive(false); // ���� �� Canvas ��Ȱ��ȭ
        _setting.SetActive(true); // ���� �� Setting Ȱ��ȭ
        _exitbutton.SetActive(false); // ���� �� button ��Ȱ��ȭ
     
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // ESC ŵ - Canvas ǥ/ɨ Ǧ (Ȱ��/��Ȱ�� ��ȯ)
            if (_canvas.activeSelf)
            {
                ExitSetting(); // Canvas ɨ
            }
            else
            {
                Setting(); // Canvas ǥ
            }
        }

        if (_canvas.activeSelf)
        {
            Time.timeScale = 0f; // ���� �Ͻ�����
        }
        else
        {
            Time.timeScale = 1f; // ���� �簳
        }
    }
    public static void ResetData()
    {
        PlayerPrefs.DeleteAll();
    }

    public void Setting()
    {
        _canvas.SetActive(true); // ���� �� Canvas Ȱ��ȭ
        _setting.SetActive(false); // ���� �� Setting ��Ȱ��ȭ
        _exitbutton.SetActive(true); // ���� �� button Ȱ��ȭ
    }

    public void ExitSetting()
    {
        _canvas.SetActive(false); // ���� �� Canvas Ȱ��ȭ
        _setting.SetActive(true); // ���� �� Setting ��Ȱ��ȭ
        _exitbutton.SetActive(false); // ���� �� button Ȱ��ȭ
    }

    public void Option()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Canvas ��� (Ȱ��/��Ȱ�� ��ȯ)
            _canvas.SetActive(false);
            _setting.SetActive(true); // Setting�� Canvas�� �ݴ��
            _exitbutton.SetActive(false);
        }
    }
}
