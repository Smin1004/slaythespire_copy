using UnityEngine;
using System.Collections;
using System.Linq;

public abstract class SkillScript
{
    public Skill skillData;

    public string FormatDesc(Skill skill, int plusValue)
    {
        string rawFormat = skill.desc;
        int[] currentValues = skill.isUpgraded ? skill.upgradeValue : skill.skillValue;

        object[] objectValues = currentValues.Select(val => val + plusValue).Cast<object>().ToArray();

        return string.Format(rawFormat, objectValues);
    }

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

public class Strike : SkillScript //타격
{
    public override IEnumerator Trigger(Entity unit, Entity[] target, int[] value)
    {
        unit.AttackEvent();
        unit.ExecuteAttack(target[0], value[0]);
        yield break;
    }
}

public class Defend : SkillScript //수비
{
    public override IEnumerator Trigger(Entity unit, Entity[] target, int[] value)
    {
        unit.ExecuteBlock(value[0]);
        yield break;
    }
}

public class Inflame : SkillScript //발화
{
    public override IEnumerator Trigger(Entity unit, Entity[] target, int[] value)
    {
        unit.UseBuff();
        Buff curBuff = DataManager.Instance.AddBuff(1, value[0]);
        unit.AddBuff(curBuff);
        yield break;
    }
}


public class Bash : SkillScript //강타
{
    public override IEnumerator Trigger(Entity unit, Entity[] target, int[] value)
    {
        unit.AttackEvent();
        unit.UseDebuff();
        unit.ExecuteAttack(target[0], value[0]);
        Buff curBuff = DataManager.Instance.AddBuff(3, value[0]);
        target[0].AddBuff(curBuff);
        yield break;
    }
}

public class Anger : SkillScript //분노
{
    public override IEnumerator Trigger(Entity unit, Entity[] target, int[] value)
    {
        unit.AttackEvent();
        unit.ExecuteAttack(target[0], value[0]);
        DeckManager.Instance.AddCard(this.skillData, 2);
        yield break;
    }
}

public class BodySlam : SkillScript //몸통박치기
{
    public override IEnumerator Trigger(Entity unit, Entity[] target, int[] value)
    {
        unit.AttackEvent();
        unit.ExecuteAttack(target[0], unit.curBlock);
        yield break;
    }
}

public class SwordBoomerang : SkillScript //부메랑칼날
{
    public override IEnumerator Trigger(Entity unit, Entity[] target, int[] value)
    {
        for (int i = 0; i < value[1]; i++)
        {
            unit.AttackEvent();
            int random = Random.Range(0, target.Length);
            unit.ExecuteAttack(target[random], value[0]);
            yield return new WaitForSeconds(0.2f);
        }
    }
}

public class SetupStrike : SkillScript //사전타격
{
    public override IEnumerator Trigger(Entity unit, Entity[] target, int[] value)
    {
        unit.AttackEvent();
        unit.ExecuteAttack(target[0], value[0]);
        unit.UseBuff();
        Buff curBuff = DataManager.Instance.AddBuff(1, value[1]);
        unit.AddBuff(curBuff);
        curBuff = DataManager.Instance.AddBuff(6, value[1]);
        unit.AddBuff(curBuff);
        yield break;
    }
}

public class TwinStrike : SkillScript //이중타격
{
    public override IEnumerator Trigger(Entity unit, Entity[] target, int[] value)
    {
        for (int i = 0; i < 2; i++)
        {
            unit.AttackEvent();
            unit.ExecuteAttack(target[0], value[0]);
            yield return new WaitForSeconds(0.2f);
        }
        yield break;
    }
}

public class Breakthrough : SkillScript //정면돌파
{
    public override IEnumerator Trigger(Entity unit, Entity[] target, int[] value)
    {
        unit.AttackEvent();
        unit.Damage(unit, 1, true);
        for (int i = 0; i < target.Length; i++)
        {
            unit.ExecuteAttack(target[i], value[0]);
        }
        yield break;
    }
}

public class ThunderClap : SkillScript //천둥
{
    public override IEnumerator Trigger(Entity unit, Entity[] target, int[] value)
    {
        unit.AttackEvent();
        Buff curBuff = DataManager.Instance.AddBuff(3, value[0]);
        
        for (int i = 0; i < target.Length; i++)
        {
            unit.ExecuteAttack(target[i], value[0]);
            target[i].AddBuff(curBuff);
        }
        yield break;
    }
}

public class IronWave : SkillScript
{
    public override IEnumerator Trigger(Entity unit, Entity[] target, int[] value)
    {
        unit.AttackEvent();
        unit.ExecuteAttack(target[0], value[1]);
        unit.ExecuteBlock(value[0]);
        yield break;
    }
}

public class PommelStrike : SkillScript
{
    public override IEnumerator Trigger(Entity unit, Entity[] target, int[] value)
    {
        unit.AttackEvent();
        unit.ExecuteAttack(target[0], value[0]);
        DeckManager.Instance.DrawCards(value[1]);
        yield break;
    }
}

public class Bloodletting : SkillScript
{
    public override IEnumerator Trigger(Entity unit, Entity[] target, int[] value)
    {
        unit.Damage(unit, 3, true);
        unit.GetComponent<Player>().energy += value[0];
        yield break;
    }
}

public class Tremble : SkillScript
{
    public override IEnumerator Trigger(Entity unit, Entity[] target, int[] value)
    {
        yield break;
    }
}

public class ShrugItOff : SkillScript
{
    public override IEnumerator Trigger(Entity unit, Entity[] target, int[] value)
    {
        unit.ExecuteBlock(value[0]);
        DeckManager.Instance.DrawCards(1);
        yield break;
    }
}

public class BloodWall : SkillScript
{
    public override IEnumerator Trigger(Entity unit, Entity[] target, int[] value)
    {
        unit.Damage(unit, 3, true);
        unit.ExecuteBlock(value[0]);
        yield break;
    }
}

