using UnityEngine;

public class PlayOption : MonoBehaviour
{
    [SerializeField] GameObject _endbutton;

    void OnEnable()
    {
        Button.OnSettingOpen += HideCardsAndEndButton;
        Button.OnSettingClose += ShowCardsAndEndButton;
    }

    void OnDisable()
    {
        Button.OnSettingOpen -= HideCardsAndEndButton;
        Button.OnSettingClose -= ShowCardsAndEndButton;
    }

    void HideCardsAndEndButton()
    {
        _endbutton.SetActive(false);
    }

    void ShowCardsAndEndButton()
    {
        _endbutton.SetActive(true);
    }
}
