using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LoadData", menuName = "GameData/LoadData")]
public class LoadData : ScriptableObject
{
    public List<Skill> SkillList = new();
    public List<Buff> BuffList = new();
}
