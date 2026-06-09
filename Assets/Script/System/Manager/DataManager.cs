using UnityEngine;

public class DataManager : MonoBehaviour
{
    private static DataManager _instance;
    public static DataManager Instance => _instance;

    public LoadData loadData;

    public void InitAwake()
    {
        if (_instance != null) Destroy(gameObject);
        else _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void InitStart()
    {
        Player.Instance.masterDeck = loadData.SkillList;
    }

    public Buff AddBuff(int index, int value)
    {
        Buff temp = loadData.BuffList[index];
        temp.effect.buffData = temp;
        temp.value = value;
        //DeckManager.Instance.UpdateDesc();
        return temp;
    }
}
