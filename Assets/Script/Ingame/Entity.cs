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

    public int CurrentHp => curHp;
    public int MaxHp => maxHp;

    // 초기화
    public virtual void InitializeEntity(int startingHealth)
    {
        maxHp = startingHealth;
        curHp = startingHealth;
        curBlock = 0;
    }

    // 데미지 처리
    public virtual int Damage(int damageAmount)
    {
        if (damageAmount <= 0) return 0;

        int hpBefore = curHp;

        if (curBlock > 0)
        {
            int remainingDamage = damageAmount - curBlock;
            curBlock = Mathf.Max(0, curBlock - damageAmount);
            OnBlockChanged?.Invoke(curBlock);

            if (remainingDamage <= 0)
            {
                return 0;
            }
            damageAmount = remainingDamage;
        }

        curHp = Mathf.Max(0, curHp - damageAmount);
        OnHealthChanged?.Invoke(curHp, maxHp);

        return hpBefore - curHp;
    }

    // 방어도 추가
    public virtual void AddBlock(int blockAmount)
    {
        curBlock += blockAmount;
        OnBlockChanged?.Invoke(curBlock);
    }
}