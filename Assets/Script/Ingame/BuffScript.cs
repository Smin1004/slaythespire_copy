using UnityEngine;

public abstract class BuffScript : MonoBehaviour
{
    public Buff buffData;

    public virtual void OnTurnStart(Entity unit) { }                                //턴 시작
    public virtual void OnCardPlayed(Entity unit, Card playedCard) { }              //카드 트리거
    public virtual int OnModifyAttack(Entity unit, int value) { return value; }     //공격
    public virtual int OnModifyBlock(Entity unit, int value) { return value; }      //피격
    public virtual void OnTakeDamage(Entity unit, Entity target) { }                //피격 이후
    public virtual void OnTurnEnd(Entity unit) { }                                 //턴 종료
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

public class strength : BuffScript
{
    public override void OnCardPlayed(Entity unit, Card playedCard) 
    {
        
    }
}
