using UnityEngine;

public class Player : Entity
{
    private static Player _instance;
    public static Player Instance => _instance;

    public int energy;
    public int maxEnergy;

    void Awake()
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
