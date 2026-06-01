using UnityEngine;
using System;

public abstract class Entity : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] protected int curHp;
    [SerializeField] protected int maxHp;
    [SerializeField] protected int curBlock;


    public event Action<int, int> OnHealthChanged;
    public event Action<int> OnBlockChanged;

    // 초기화
    public virtual void InitializeEntity(int startingHealth)
    {
        maxHp = startingHealth;
        curHp = startingHealth;
        curBlock = 0;
    }

    // 데미지 처리
    public virtual void Damage(int damageAmount)
    {
        if (curBlock > 0)
        {
            int remainingDamage = damageAmount - curBlock;
            curBlock = Mathf.Max(0, curBlock - damageAmount);
            OnBlockChanged?.Invoke(curBlock);

            if (remainingDamage <= 0) return;
            damageAmount = remainingDamage;
        }

        curHp = Mathf.Max(0, curHp - damageAmount);
        OnHealthChanged?.Invoke(curHp, maxHp);
    }

    // 방어도 추가
    public virtual void AddBlock(int blockAmount)
    {
        curBlock += blockAmount;
        OnBlockChanged?.Invoke(curBlock);
    }
}