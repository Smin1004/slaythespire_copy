using UnityEngine;

[System.Serializable]
public class Skill
{
    public int index;
    public string name;
    public SkillType type;

    public int cost;
    public int upgradeCost;

    public int[] skillValue;
    public int[] upgradeValue;
    
    public SkillScript effect;
    public string skill_desc;
}

public enum SkillType
{
    Attack,
    Skill,
    Power,
}

public enum Keyword
{
    //보존 / 소멸 / 재사용 같은 카드에 붙혀진 키워드에 대한 판정
    //근데 이 부분까지 구현될지는 모르겠어서 일단 형태만 냅두고 되면 추가할 생각
    test,
}

public class DataManager : MonoBehaviour
{
    private static DataManager _instance;
    public static DataManager Instance => _instance;

    public LoadData loadData;

    private void Awake()
    {
        if (_instance != null) Destroy(gameObject);
        else _instance = this;
        DontDestroyOnLoad(gameObject);
    }

}
