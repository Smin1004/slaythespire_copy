using UnityEngine;

public class TimingController : MonoBehaviour
{
    [SerializeField] Player player;
    [SerializeField] ReadCSV readCSV;
    [SerializeField] DataManager dataManager;
    [SerializeField] DeckManager deckManager;
    [SerializeField] BattleManager battleManager;
    [SerializeField] RewardManager rewardManager;

    void Awake()
    {
        dataManager.InitAwake();
        readCSV.InitAwake();
        deckManager.InitAwake();
        battleManager.InitAwake();
        player.InitAwake();
        rewardManager.InitAwake();
    }

    void Start()
    {
        readCSV.InitStart();
        dataManager.InitStart();
        battleManager.InitStart();
    }
}
