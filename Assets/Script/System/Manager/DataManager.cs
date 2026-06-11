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
        if (loadData == null)
            return null;

        Buff template = loadData.GetBuffByIndex(index);
        if (template == null && index >= 0 && index < loadData.BuffList.Count)
            template = loadData.BuffList[index];

        if (template == null)
            return null;

        return template.CreateRuntimeCopy(value, 1);
    }
}
