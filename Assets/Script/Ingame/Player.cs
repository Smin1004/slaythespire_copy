using System.Collections.Generic;
using UnityEngine;

public class Player : Entity
{
    private static Player _instance;
    public static Player Instance => _instance;

    public List<Skill> masterDeck = new List<Skill>();

    public int energy;
    public int maxEnergy;

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

    public void playerTurnInit()
    {
        energy = maxEnergy;
    }
}
