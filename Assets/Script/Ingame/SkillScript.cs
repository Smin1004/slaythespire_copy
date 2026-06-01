using UnityEngine;

public abstract class SkillScript
{
    public virtual void Setting(Entity unit, Entity target) { }
    public virtual void End(Entity unit, Entity target) { }
}
