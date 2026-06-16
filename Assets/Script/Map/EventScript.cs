using System.Security.Cryptography;
using UnityEngine;

public abstract class EventScript
{
    // 각 이벤트가 덮어써서 구현할 실제 행동 로직
    public abstract void ExecuteAction(int value);
}

public class Heal : EventScript
{
    public override void ExecuteAction(int value)
    {
        if (value == 0) value = 10;
        Player.Instance.Heal(value);
    }
}

public class Damage : EventScript
{
    public override void ExecuteAction(int value)
    {
        if (value == 0) value = 10;
        Player.Instance.Damage(null, value, true);
    }
}

public class Next : EventScript
{
    public override void ExecuteAction(int value)
    {

    }
}