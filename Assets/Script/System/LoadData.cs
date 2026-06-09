using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LoadData", menuName = "GameData/LoadData")]
public class LoadData : ScriptableObject
{
    public List<Skill> SkillList = new();
    public List<Buff> BuffList = new();

    public Buff GetBuffByIndex(int index)
    {
        return BuffList.Find(buff => buff != null && buff.index == index);
    }

    public Buff GetBuffByName(string buffName)
    {
        return BuffList.Find(buff => buff != null && buff.name == buffName);
    }

    public Buff GetBuffByKey(string key)
    {
        return BuffList.Find(buff => buff != null && buff.key == key);
    }
}
