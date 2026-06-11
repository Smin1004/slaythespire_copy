using System;
using UnityEngine;

public abstract class BuffScript
{
    public Buff buffData;

    public virtual void OnTurnStart(Entity unit, int value) { }                              // 턴 시작 시 실행되는 효과입니다.
    public virtual int OnTrigger(Entity unit, Skill playedCard, int value) { return value; } // 카드 사용 시 카드 수치에 적용되는 효과입니다.
    public virtual int OnAttack(Entity unit, int value) { return value; }                    // 실제 공격 계산 시 공격자에게 적용되는 효과입니다.

    public virtual int OnBlock(Entity unit, int value) { return value; }                     // 실제 피격 계산 시 방어자에게 적용되는 효과입니다.
    public virtual void OnAfter(Entity unit, Entity target) { }                              // 피격 계산이 끝난 뒤 실행되는 효과입니다.
    public virtual void OnTurnEnd(Entity unit, int value) { }                                // 턴 종료 시 실행되는 효과입니다.
}

[System.Serializable]
public class Buff
{
    public int index;
    public string name;
    public string key;

    public bool isDebuff;
    public int value;          // 효과 수치입니다. 예: 힘 +2, 취약 적용량 등입니다.
    public int remainingTurns; // 지속 턴입니다. value와 분리해서 효과 수치가 턴 처리 때문에 꼬이지 않게 합니다.
    public bool isPermanent;   // 영구적 버프
    public bool skipNextTurnTick;   // 적 캐릭터 턴종료시 디버프 없어지게하는 용도

    public Sprite img;
    public BuffScript effect;
    public string desc;

    public Buff CreateRuntimeCopy(int runtimeValue, int runtimeTurns = 1)
    {
        Buff copy = new Buff
        {
            index = index,
            name = name,
            key = key,
            isDebuff = isDebuff,
            skipNextTurnTick = skipNextTurnTick,
            isPermanent = isPermanent,
            value = runtimeValue,
            remainingTurns = isPermanent ? -1 : Mathf.Max(1, runtimeTurns),
            img = img,
            desc = desc
        };

        if (effect != null)
            copy.effect = Activator.CreateInstance(effect.GetType()) as BuffScript;

        if (copy.effect != null)
            copy.effect.buffData = copy;

        return copy;
    }
}

public class strength : BuffScript // 힘
{
    public override int OnTrigger(Entity unit, Skill skill, int value)
    {
        if (skill.type != SkillType.Attack) return value;
        return value + buffData.value;
    }

    public override int OnAttack(Entity unit, int value)
    {
        // 몬스터는 카드를 쓰지 않아서 OnTrigger를 타지 않습니다. 공격 계산에도 힘을 적용해 몬스터 버프가 동작하게 합니다.
        if (unit is Player) return value;
        return value + buffData.value;
    }
}

public class dexterity : BuffScript // 민첩
{
    public override int OnTrigger(Entity unit, Skill skill, int value)
    {
        if (skill.type != SkillType.Skill) return value;
        return value + buffData.value;
    }
}

public class vulnerable : BuffScript // 취약
{
    public override int OnBlock(Entity unit, int value)
    {
        value = Mathf.FloorToInt(value * 1.5f);
        return value;
    }
}

public class weak : BuffScript // 약화
{
    public override int OnAttack(Entity unit, int value)
    {
        value = Mathf.FloorToInt(value * 0.75f);
        return value;
    }
}

public class frail : BuffScript // 손상
{
    public override int OnTrigger(Entity unit, Skill skill, int value)
    {
        if (skill.type != SkillType.Skill) return value;
        value = Mathf.FloorToInt(value * 0.75f);
        return value;
    }
}

public class TemporaryStrength : BuffScript // 기간제 힘
{
    public override void OnTurnEnd(Entity unit, int value)
    {
        unit.buffs.Find(item => item.index == 1).value -= value;
    }
}