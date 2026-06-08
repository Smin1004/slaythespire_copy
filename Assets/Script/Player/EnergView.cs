using TMPro;
using UnityEngine;

public class EnergView : MonoBehaviour
{
    [SerializeField] private Player _player;
    [SerializeField] private TMP_Text _energyText;

    private void OnEnable()
    {
        _player.OnEnergyChanged += UpdateEnergy;
    }

    private void OnDisable()
    {
        _player.OnEnergyChanged -= UpdateEnergy;
    }

    private void Start()
    {
        UpdateEnergy(_player.energy, _player.maxEnergy);
    }

    private void UpdateEnergy(int current, int max)
    {
        _energyText.text = $"{current}/{max}";
    }
}