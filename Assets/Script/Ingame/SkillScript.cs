using UnityEngine;
using System.Collections;

public abstract class SkillScript
{
    public abstract IEnumerator Trigger(Entity unit, Entity[] target, int[] value);
}

public class testAttack : SkillScript
{
    public override IEnumerator Trigger(Entity unit, Entity[] target, int[] value)
    {
        target[0].Damage(value[0]);
        yield break;
    }
}

public class testPlusAttack : SkillScript
{
    public override IEnumerator Trigger(Entity unit, Entity[] target, int[] value)
    {
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
        //target[0].Damage(value[0]);
        Debug.Log("아직 버프가 업성요");
        yield break;
    }
}
