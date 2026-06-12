using Unity.VisualScripting;
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
        if (loadData == null ||index < 0 || index >= loadData.BuffList.Count) return null;

        Buff temp = loadData.BuffList[index];
        Buff newBuff = new Buff(temp);
        newBuff.value = value;

        return newBuff;
    }
}
