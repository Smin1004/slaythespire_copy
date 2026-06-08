using UnityEngine;

public abstract class SkillScript
{
    public virtual void Trigger(Entity unit, Entity[] target, int[] value) { }
}

public class testAttack : SkillScript
{
    public override void Trigger(Entity unit, Entity[] target, int[] value)
    {
        unit.Attack();  //애니메이션

        target[0].Damage(value[0]);
    }
}

public class testPlusAttack : SkillScript
{
    public override void Trigger(Entity unit, Entity[] target, int[] value)
    {
        unit.Attack();  //애니메이션 구현

        for (int i = 0; i < value[1]; i++)
        {
            int random = Random.Range(0, target.Length);
            target[random].Damage(value[0]);
        }
    }
}

public class testSkill : SkillScript
{
    public override void Trigger(Entity unit, Entity[] target, int[] value)
    {
        unit.AddBlock(value[0]);
    }
}

public class testPower : SkillScript
{
    public override void Trigger(Entity unit, Entity[] target, int[] value)
    {
        //target[0].Damage(value[0]);
        Debug.Log("아직 버프가 업성요");
    }
}
