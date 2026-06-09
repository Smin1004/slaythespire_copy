using UnityEngine;

public abstract class BuffScript
{
    public Buff buffData;
    
    public virtual void OnTurnStart(Entity unit) { }                                                //턴 시작
    public virtual int OnTrigger(Entity unit, Skill playedCard, int value) { return value; }        //카드 트리거
    public virtual int OnAttack(Entity unit, int value) { return value; }                           //공격
    public virtual int OnBlock(Entity unit, int value) { return value; }                            //피격
    public virtual void OnAfter(Entity unit, Entity target) { }                                     //피격 이후
    public virtual void OnTurnEnd(Entity unit) { }                                                  //턴 종료
}

[System.Serializable]
public class Buff
{
    public int index;
    public string name;
    public string key;

    public bool isDebuff;
    public int value;

    public Sprite img;
    public BuffScript effect;
    public string desc;
}

public class strength : BuffScript //힘
{
    public override int OnTrigger(Entity unit, Skill skill, int value)
    {
        if (skill.type != SkillType.Attack) return value;
        return value + buffData.value;
    }
}

public class dexterity : BuffScript //민첩
{
    public override int OnTrigger(Entity unit, Skill skill, int value)
    {
        if (skill.type != SkillType.Skill) return value;
        return value + buffData.value;
    }
}

public class vulnerable : BuffScript //취약
{
    public override int OnBlock(Entity unit, int value)
    {
        value = Mathf.FloorToInt(value * 0.5f);
        return value;
    }
}

public class weak : BuffScript //약화
{
    public override int OnAttack(Entity unit, int value)
    {
        value = Mathf.FloorToInt(value * 0.25f);
        return value;
    }
}

public class frail : BuffScript //손상
{
    public override int OnTrigger(Entity unit, Skill skill, int value)
    {
        if (skill.type != SkillType.Attack) return value;
        value = Mathf.FloorToInt(value * 0.25f);
        return value;
    }
}