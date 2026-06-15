using Unity.VisualScripting;
using System.Collections.Generic;
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
        // Give the player a separate deck list so rewards do not modify LoadData.
        Player.Instance.masterDeck = loadData != null
            ? new List<Skill>(loadData.SkillList)
            : new List<Skill>();
    }

    public Buff GetBuff(int index, int value)
    {
        if (loadData == null ||index < 0 || index >= loadData.BuffList.Count) return null;

        Buff temp = loadData.BuffList[--index];
        Buff newBuff = new(temp) {value = value};

        return newBuff;
    }
}
