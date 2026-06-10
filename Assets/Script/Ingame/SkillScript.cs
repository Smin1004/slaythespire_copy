using UnityEngine;
using System.Collections;
using System.Linq;

public abstract class SkillScript
{
    public abstract IEnumerator Trigger(Entity unit, Entity[] target, int[] value);

    public string FormatDesc(Skill skill, int plusValue)
    {
        string rawFormat = skill.desc;
        int[] currentValues = skill.isUpgraded ? skill.upgradeValue : skill.skillValue;

        object[] objectValues = currentValues.Select(val => val + plusValue).Cast<object>().ToArray();

        return string.Format(rawFormat, objectValues);
    }
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
        unit.AttackEvent();  //애니메이션

        if (target == null || target.Length == 0 || target[0] == null)
            yield break; // 공격 대상이 없으면 target[0] 접근으로 오류가 나므로 효과를 중단합니다.

        unit.ExecuteAttack(target[0], value[0]);
        yield break;
    }
}

public class testPlusAttack : SkillScript
{
    public override IEnumerator Trigger(Entity unit, Entity[] target, int[] value)
    {
        unit.AttackEvent();  //애니메이션 구현

        for (int i = 0; i < value[1]; i++)
        {
            if (target == null || target.Length == 0)
                yield break; // 랜덤 공격도 대상 배열이 비어 있으면 Random.Range/배열 접근 오류가 납니다.

            Debug.Log("Plus Attack");
            int random = Random.Range(0, target.Length);
            yield return new WaitForSeconds(0.2f);
            unit.ExecuteAttack(target[random], value[0]);
        }
    }
}

public class testSkill : SkillScript
{
    public override IEnumerator Trigger(Entity unit, Entity[] target, int[] value)
    {
        unit.ExecuteBlock(value[0]);
        yield break;
    }
}

public class testPower : SkillScript
{
    public override IEnumerator Trigger(Entity unit, Entity[] target, int[] value)
    {
        unit.UseBuff();
        Buff curBuff = DataManager.Instance.AddBuff(0, value[0]);
        unit.AddBuff(curBuff);
        yield break;
    }
}
