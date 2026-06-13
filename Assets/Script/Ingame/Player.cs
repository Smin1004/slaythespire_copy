using System.Collections.Generic;
using System;
using UnityEngine;

public class Player : Entity
{
    private static Player _instance;
    public static Player Instance => _instance;

    public List<Skill> masterDeck = new List<Skill>();
    public event Action<int, int> OnEnergyChanged;

    public int energy;
    public int maxEnergy;

    public int gold;
    //public bool isMoveTime;

    void Awake()
    {
        
    }

    public void InitAwake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space)) AddBuff(DataManager.Instance.GetBuff(0, 1));
    }

    public override void TurnInit()
    {
        base.TurnInit();
        energy = maxEnergy;
        OnEnergyChanged?.Invoke(energy, maxEnergy);
    }

    public virtual int BuffCheck_CardTrigger(Skill skill, int value)
    {
        for(int i = 0; i < buffs.Count; i++)
        {
            Buff buff = buffs[i];
            if (buff?.effect == null)
                continue;

            buff.effect.buffData = buff;
            value = buff.effect.OnTrigger(this, skill, value);
        }
        return value;
    }


    public void UseEnergy(int amount)
    {
        energy -= amount;

        if (energy < 0)
            energy = 0;

        OnEnergyChanged?.Invoke(energy, maxEnergy);
    }

    public int Heal(int amount)
    {
        if (amount <= 0 || IsDead)
            return 0;

        int beforeHp = curHp;
        curHp = Mathf.Min(maxHp, curHp + amount);

        return curHp - beforeHp;
    }
}
