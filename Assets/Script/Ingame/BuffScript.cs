using UnityEngine;

public abstract class BuffScript : MonoBehaviour
{
    public Buff buffData;

    public virtual void OnTurnStart(Entity ownerUnit) { }
    public virtual void OnTurnEnd(Entity ownerUnit) { }
    public virtual void OnTakeDamage(Entity ownerUnit, int damageAmount) { }
}

[System.Serializable]
public class Buff
{
    public int index;
    public string name;

    public bool isDebuff;
    public int value;

    public Sprite img;
    public BuffScript effect;
    public string desc;
}
