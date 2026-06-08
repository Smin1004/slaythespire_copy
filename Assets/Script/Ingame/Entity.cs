using System;
using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] protected int curHp;     // 현재 체력입니다.
    [SerializeField] protected int maxHp;     // 최대 체력입니다.
    [SerializeField] protected int curBlock;  // 먼저 데미지를 막아주는 방어도입니다.
    [SerializeField] private bool _isDead;    // Entity가 죽었는지 여부입니다. 죽은 Entity는 행동할 수 없습니다.

    public event Action<int, int> OnHealthChanged;  // HP바처럼 체력 UI를 갱신할 때 사용합니다. (현재 HP, 최대 HP)
    public event Action<int> OnBlockChanged;        // 방어도 UI가 생기면 연결해서 쓸 수 있습니다.
    public event Action<int> OnDamaged;             // 실제 HP가 깎였을 때만 실행됩니다. 데미지 텍스트, SFX, 카메라 효과가 여기에 붙습니다.
    public event Action OnAttack;              // 공격을 할때 실행함
    public event Action OnDead;                     // Entity가 죽었을 때 실행됩니다.
    public event Action OnRevived;


    public int CurrentHp => curHp;
    public int MaxHp => maxHp;
    public int CurrentBlock => curBlock;
    public bool IsDead => _isDead;

    public virtual void InitializeEntity(int startingHealth)
    {
        maxHp = startingHealth;
        curHp = startingHealth;
        curBlock = 0;
        OnHealthChanged?.Invoke(curHp, maxHp);
        OnBlockChanged?.Invoke(curBlock);
    }

    //Attack 이벤트 
    public virtual void Attack()
    {
        OnAttack?.Invoke();
    }

    public virtual void Damage(int damageAmount)
    {
        if (damageAmount <= 0)
            return;

        if (_isDead)
            return;
        
        // 방어도가 있으면 방어도를 먼저 깎고, 남은 데미지만 HP에 적용합니다.
        if (curBlock > 0)
        {
            int remainingDamage = damageAmount - curBlock;
            curBlock = Mathf.Max(0, curBlock - damageAmount);
            OnBlockChanged?.Invoke(curBlock);

            if (remainingDamage <= 0)
                return;

            damageAmount = remainingDamage;
        }

        int beforeHp = curHp;
        curHp = Mathf.Max(0, curHp - damageAmount);
        OnHealthChanged?.Invoke(curHp, maxHp);

        // 방어도에 막힌 값이 아니라 실제로 HP가 깎인 양만 피격 이벤트로 보냅니다.
        int appliedDamage = beforeHp - curHp;
        if (appliedDamage > 0)
            OnDamaged?.Invoke(appliedDamage);

      
        if (curHp <= 0 && !_isDead)
        {
            _isDead = true;
            OnDead?.Invoke();
        }
    }

    // 체력이 0이 된 후에만 부활이 가능합니다. 부활 시 체력은 최대 체력으로 회복됩니다.
    public virtual void Revive()
    {
        _isDead = false;

        curHp = maxHp;
        curBlock = 0;

        OnHealthChanged?.Invoke(curHp, maxHp);
        OnBlockChanged?.Invoke(curBlock);
        OnRevived?.Invoke();
    }

    public virtual void AddBlock(int blockAmount)
    {
        curBlock += blockAmount;
        OnBlockChanged?.Invoke(curBlock);
    }
}
