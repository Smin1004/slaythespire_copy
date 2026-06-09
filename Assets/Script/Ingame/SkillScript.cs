using UnityEngine;
using System.Collections;

public abstract class SkillScript
{
    public abstract IEnumerator Trigger(Entity unit, Entity[] target, int[] value);
}

[System.Serializable]
public class Skill
{
    public int index;
    public string name;
    public SkillType type;
    public bool isTargeting;

    public int cost;
    public int upgradeCost;

    public int[] skillValue;
    public int[] upgradeValue;
    
    public Sprite img;
    public SkillScript effect;
    public string desc;
    public bool isUpgraded;
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

public class testAttack : SkillScript
{
    public override IEnumerator Trigger(Entity unit, Entity[] target, int[] value)
    {
        unit.Attack();  //애니메이션

        target[0].Damage(value[0]);
        yield break;
    }
}

public class testPlusAttack : SkillScript
{
    public override IEnumerator Trigger(Entity unit, Entity[] target, int[] value)
    {
        unit.Attack();  //애니메이션 구현

        for (int i = 0; i < value[1]; i++)
        {
            Debug.Log("Plus Attack");   
            int random = Random.Range(0, target.Length);
            yield return new WaitForSeconds(0.2f);
            target[random].Damage(value[0]);
        }
    }
}

public class testSkill : SkillScript
{
    public override IEnumerator Trigger(Entity unit, Entity[] target, int[] value)
    {
        unit.AddBlock(value[0]);
        yield break;
    }
}

public class testPower : SkillScript
{
    public override IEnumerator Trigger(Entity unit, Entity[] target, int[] value)
    {
        unit.UseBuff();
        //target[0].Damage(value[0]);
        Debug.Log("아직 버프가 업성요");
        yield break;
    }
}
