using UnityEngine;

public class TimingController : MonoBehaviour
{
    [SerializeField] Player player;
    [SerializeField] ReadCSV readCSV;
    [SerializeField] DataManager dataManager;
    [SerializeField] DeckManager deckManager;
    [SerializeField] BattleManager battleManager;

    void Awake()
    {
        dataManager.InitAwake();
        readCSV.InitAwake();
        deckManager.InitAwake();
        battleManager.InitAwake();
        player.InitAwake();
    }

    void Start()
    {
        readCSV.InitStart();
        dataManager.InitStart();
        battleManager.InitStart();
    }
}
