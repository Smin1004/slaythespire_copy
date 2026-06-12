using UnityEngine;

public class TimingController : MonoBehaviour
{
    [SerializeField] Player player;
    [SerializeField] ReadCSV readCSV;
    [SerializeField] DataManager dataManager;
    [SerializeField] DeckManager deckManager;
    [SerializeField] BattleManager battleManager;
    [SerializeField] RewardManager rewardManager;
    [SerializeField] RunManager runManager; // 맵 진행 관리자

    void Awake()
    {
        dataManager.InitAwake();
        readCSV.InitAwake();
        deckManager.InitAwake();
        battleManager.InitAwake();
        player.InitAwake();
        rewardManager.InitAwake();
        runManager.InitAwake();
    }

    void Start()
    {
        readCSV.InitStart();
        dataManager.InitStart();

        // BattleManager.InitStart()는 이제 전투를 바로 시작하지 않습니다.
        battleManager.InitStart();

        // 여기서 첫 맵 선택지를 보여줍니다.
        runManager.InitStart();
    }
}
