using System;
using UnityEngine;
using System.Collections.Generic;

public abstract class Entity : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] public int curHp;     // 현재 체력입니다.
    [SerializeField] public int maxHp;     // 최대 체력입니다.
    [SerializeField] public int curBlock;  // 먼저 데미지를 막아주는 방어도입니다.
    [SerializeField] private bool _isDead;    // Entity가 죽었는지 여부입니다. 죽은 Entity는 행동할 수 없습니다.

    [Header("SFX")]
    [SerializeField] protected AudioClip attackSound;
    [SerializeField] protected AudioClip blockGainSound;
    [SerializeField] protected AudioClip blockHitSound;
    [SerializeField] protected AudioClip buffSound;
    [SerializeField] protected AudioClip debuffSound;

    public event Action<int, int> OnHealthChanged;  // HP바처럼 체력 UI를 갱신할 때 사용합니다. (현재 HP, 최대 HP)
    public event Action<int> OnBlockChanged;        // 방어도 UI가 생기면 연결해서 쓸 수 있습니다.
    public event Action<int> OnDamaged;             // 실제 HP가 깎였을 때만 실행됩니다. 데미지 텍스트, SFX, 카메라 효과가 여기에 붙습니다.
    public event Action<int> OnHitReceived;         // 방어도에 막혀도 공격이 닿았을 때 실행
    public event Action OnAttack;                   // 공격을 할때 실행함
    public event Action OnDead;                     // Entity가 죽었을 때 실행됩니다.
    public event Action<IReadOnlyList<Buff>> OnBuffsChanged;

    public List<Buff> buffs = new List<Buff>();
    public IReadOnlyList<Buff> Buffs => buffs;

    public int CurrentHp => curHp;
    public int MaxHp => maxHp;
    public int CurrentBlock => curBlock;
    public bool IsDead => _isDead;

    protected virtual void InitializeEntity(int startingHealth)
    {
        maxHp = startingHealth;
        curHp = startingHealth;
        curBlock = 0;
        OnHealthChanged?.Invoke(curHp, maxHp);
        OnBlockChanged?.Invoke(curBlock);
    }

    protected void NotifyHealthChanged()
    {
        // 하위 클래스가 HP를 직접 바꾼 뒤 UI를 갱신할 수 있게 합니다.
        OnHealthChanged?.Invoke(curHp, maxHp);
    }

    public virtual void TurnInit()
    {
        for (int i = buffs.Count - 1; i >= 0; i--)
        {
            Buff buff = buffs[i];
            if (buff?.effect == null)
                continue;

            buff.effect.buffData = buff;
            buff.effect.OnTurnStart(this, buffs[i].value);
        }
        curBlock = 0;
        OnBlockChanged?.Invoke(curBlock);
    }

    //공격시 버프 연산
    public int ExecuteAttack(Entity target, int value, bool isCheck = false)
    {
        if (target == null || IsDead) return value;

        value = BuffCheck_Attack(this, value, isCheck);
        value = target.BuffCheck_Block(target, value, isCheck);
        if(isCheck) return value;

        AttackEvent();
        target.Damage(this, value);
        return value;
    }

    //방어시 버프 연산
    public int ExecuteBlock(int value, bool isCheck = false)
    {
        value = BuffCheck_Attack(this, value);
        if(isCheck) return value;

        DefendEvent();
        AddBlock(value);
        return value;
    }

    //버프, 디버프 부여
    public virtual void AddBuff(Buff buff)
    {
        if (buff == null) return;
            
        Buff existingBuff = buffs.Find(item => item != null && item.index == buff.index);
        if(buff.isDebuff) UseDebuff();
        else UseBuff();
        
        if (existingBuff != null)
        {
            existingBuff.value += buff.value;
            OnBuffsChanged?.Invoke(Buffs);
            return;
        }

        buffs.Add(buff);
        OnBuffsChanged?.Invoke(Buffs);
    }

    //피격시 버프 계산
    public virtual int BuffCheck_Attack(Entity unit, int value, bool isCheck = false)
    {
        for (int i = 0; i < buffs.Count; i++)
        {
            Buff buff = buffs[i];
            if (buff?.effect == null)
                continue;

            buff.effect.buffData = buff;
            value += buff.effect.OnAttack(this, value);
        }
        return value;
    }

    //피격시 버프 계산
    public virtual int BuffCheck_Block(Entity unit, int value, bool isCheck = false)
    {
        for (int i = 0; i < buffs.Count; i++)
        {
            Buff buff = buffs[i];
            if (buff?.effect == null)
                continue;

            buff.effect.buffData = buff;
            value += buff.effect.OnBlock(this, value);
        }
        return value;
    }

    public virtual void BuffCheck_After(Entity target)
    {
        for (int i = 0; i < buffs.Count; i++)
        {
            Buff buff = buffs[i];
            if (buff?.effect == null)
                continue;

            buff.effect.buffData = buff;
            buff.effect.OnAfter(this, target);
        }
    }

    public virtual void Damage(Entity target, int damageAmount, bool isTrueDamage = false)
    {
        if (damageAmount < 0)
            return;

        if (_isDead)
            return;

        // 방어도에 막히더라도 "공격이 닿은 순간" 실행
        OnHitReceived?.Invoke(damageAmount);


        // 방어도가 있으면 방어도를 먼저 깎고, 남은 데미지만 HP에 적용합니다.
        if (!isTrueDamage && curBlock > 0)
        {
            int remainingDamage = damageAmount - curBlock;
            int blockedDamage = Mathf.Min(damageAmount, curBlock);
            curBlock = Mathf.Max(0, curBlock - damageAmount);
            OnBlockChanged?.Invoke(curBlock);

            if (blockedDamage > 0)
                AudioManager.Instance.PlaySfx(blockHitSound);

            if (remainingDamage <= 0)
            {
                BuffCheck_After(target);
                return;
            }

            damageAmount = remainingDamage;
        }
    
        int beforeHp = curHp;
        curHp = Mathf.Max(0, curHp - damageAmount);
        OnHealthChanged?.Invoke(curHp, maxHp);

        // 방어도에 막힌 값이 아니라 실제로 HP가 깎인 양만 피격 이벤트로 보냅니다.
        int appliedDamage = beforeHp - curHp;
        if (appliedDamage > 0)
            OnDamaged?.Invoke(appliedDamage);

        if(!isTrueDamage) BuffCheck_After(target);


        if (curHp <= 0 && !_isDead)
        {
            _isDead = true;
            Die();
        }
    }

    public virtual void AddBlock(int blockAmount)
    {
        if (blockAmount <= 0)
            return;

        curBlock += blockAmount;
        OnBlockChanged?.Invoke(curBlock);
        AudioManager.Instance.PlaySfx(blockGainSound);
    }

    protected virtual void Die()
    {
        OnDead?.Invoke();
    }

    //Attack 이벤트 
    public virtual void AttackEvent()
    {
        OnAttack?.Invoke();
        AudioManager.Instance.PlaySfx(attackSound);
    }

    public virtual void DefendEvent()
    {
        //방어도 얻을때 애니메이션 / 소리 있어야하는데 얘 왜 없어요
    }

    public virtual void UseBuff()
    {
        AudioManager.Instance.PlaySfx(buffSound);
    }

    public virtual void UseDebuff()
    {
        AudioManager.Instance.PlaySfx(debuffSound);
    }
}
