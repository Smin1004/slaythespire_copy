using UnityEngine;

public class PlayOption : MonoBehaviour
{
    [SerializeField] GameObject _endbutton;

    void OnEnable()
    {
        OptionButton.OnSettingOpen += HideCardsAndEndButton;
        OptionButton.OnSettingClose += ShowCardsAndEndButton;
    }

    void OnDisable()
    {
        OptionButton.OnSettingOpen -= HideCardsAndEndButton;
        OptionButton.OnSettingClose -= ShowCardsAndEndButton;
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
