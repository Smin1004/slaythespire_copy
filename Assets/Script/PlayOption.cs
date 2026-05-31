using UnityEngine;

public class PlayOption : MonoBehaviour
{
    [SerializeField] GameObject _cards;
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
        _cards.SetActive(false);
        _endbutton.SetActive(false);
    }

    void ShowCardsAndEndButton()
    {
        _cards.SetActive(true);
        _endbutton.SetActive(true);
    }
}
