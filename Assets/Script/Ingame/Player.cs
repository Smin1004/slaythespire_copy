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

    public override void TurnInit()
    {
        base.TurnInit();
        energy = maxEnergy;
        OnEnergyChanged?.Invoke(energy, maxEnergy);
    }

    public void UseEnergy(int amount)
    {
        energy -= amount;

        if (energy < 0)
            energy = 0;

        OnEnergyChanged?.Invoke(energy, maxEnergy);
    }

}
