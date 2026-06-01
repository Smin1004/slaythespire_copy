using UnityEngine;

public class Player : Entity
{
    private static Player _instance;
    public static Player Instance => _instance;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }
}
