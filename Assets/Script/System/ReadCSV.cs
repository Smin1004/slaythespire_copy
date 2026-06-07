using System;
using System.Collections.Generic;
using UnityEngine;

public class ReadCSV : MonoBehaviour
{
    private static ReadCSV _instance;
    public static ReadCSV Instance => _instance;

    [SerializeField] private TextAsset skill;
    List<Skill> skillLists = new List<Skill>();

    public void InitAwake()
    {
        if (_instance != null) Destroy(gameObject);
        else _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void InitStart()
    {
        Load();
    }

    public void Load(Action callBack = default)
    {
        LoadData(skill.text, ParseSkillData);
        //DataManager.instance.readEnd = true;    
        callBack?.Invoke();
    }

    private void LoadData(string csv, Action<string> action)
    {
        action?.Invoke(csv);
    }

    public void ParseSkillData(string data)
    {
        Debug.Log("Read");

        var d = DataManager.Instance;
        string[] rows = data.Split('\n');

        skillLists.Clear();
        for (int i = 1; i < rows.Length; i++)
        {
            Debug.Log(rows[i]);
            string[] cols = rows[i].Split(',');
            if (cols.Length < 7) continue;

            Skill skill = new();
            skill.index = int.Parse(cols[0]);
            skill.name = cols[1];
            skill.type = cols[2].EnumParse<SkillType>();
            skill.cost = int.Parse(cols[3]);
            skill.upgradeCost = int.Parse(cols[4]);
            skill.skill_desc = cols[6];
            skill.skillValue = Array.ConvertAll(cols[7].Split('!'), int.Parse);
            skill.upgradeValue = Array.ConvertAll(cols[8].Split('!'), int.Parse);
            skill.isTargeting = bool.Parse(cols[9]);
            
            string className = "Skill_" + cols[6];
            try
            {
                skill.effect = Activator.CreateInstance(Type.GetType(className)) as SkillScript;
            }
            catch
            {
                //skill.effect = Activator.CreateInstance(Type.GetType("Skill_")) as SkillScript; 
            }
            
            skillLists.Add(skill);
        }
        d.loadData.SkillList = skillLists;
    }
}