using UnityEngine;
using System.Collections;

public abstract class PoolableObject : MonoBehaviour
{
    private GameObject myPrefab;

    public void SetPrefab(GameObject prefab)
    {
        myPrefab = prefab;
    }

    public void ReturnToPoolAfterTime(float time)
    {
        Invoke("ReturnToPool", time);
    }

    public void ReturnToPool()
    {
        ObjectPoolManager.Instance.Despawn(this.gameObject, myPrefab);
    }
}