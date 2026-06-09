using System;
using UnityEngine;
using System.Collections.Generic;

public abstract class Entity : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] protected int curHp;     // 현재 체력입니다.
    [SerializeField] protected int maxHp;     // 최대 체력입니다.
    [SerializeField] protected int curBlock;  // 먼저 데미지를 막아주는 방어도입니다.
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
    public event Action OnAttack;              // 공격을 할때 실행함
    public event Action OnDead;                     // Entity가 죽었을 때 실행됩니다.
    public event Action OnRevived;
    public event Action<IReadOnlyList<Buff>> OnBuffsChanged;

    public List<Buff> buffs = new List<Buff>();
    public IReadOnlyList<Buff> Buffs => buffs;

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

    public virtual void TurnInit()
    {
        for (int i = buffs.Count - 1; i >= 0; i--)
        {
            buffs[i].effect.OnTurnStart(this);
        }
    }
    //공격시 버프 연산
    public void ExecuteAttack(Entity target, int value, bool isAttack = true)
    {
        value = BuffCheck_Attack(this, value);
        value = target.BuffCheck_Block(target, value);
        if(!isAttack) return;
        Debug.Log($"Attack!!!!! {value}");
        target.Damage(this, value);
    }

    //방어시 버프 연산
    public void ExecuteBlock(int value)
    {
        value = BuffCheck_Attack(this, value);
        AddBlock(value);
    }

    //피격시 버프 계산
    public virtual int BuffCheck_Attack(Entity unit, int value)
    {
        for(int i = 0; i < buffs.Count; i++) value = buffs[i].effect.OnAttack(this, value);
        return value;
    }

    //피격시 버프 계산
    public virtual int BuffCheck_Block(Entity unit, int value)
    {
        for(int i = 0; i < buffs.Count; i++) value = buffs[i].effect.OnBlock(this, value);
        return value;
    }

    public virtual void BuffCheck_After(Entity target)
    {
        for(int i = 0; i < buffs.Count; i++) buffs[i].effect.OnAfter(this, target);
    }

    public virtual void AddBuff(Buff buff)
    {
        if (buff == null)
            return;

        Buff existingBuff = buffs.Find(item =>
            item != null &&
            ((!string.IsNullOrEmpty(item.key) && item.key == buff.key) ||
             (item.index != 0 && item.index == buff.index)));

        if (existingBuff != null)
        {
            existingBuff.value += Mathf.Max(1, buff.value);
            OnBuffsChanged?.Invoke(Buffs);
            return;
        }

        if (buff.value <= 0)
            buff.value = 1;

        buffs.Add(buff);
        OnBuffsChanged?.Invoke(Buffs);
    }

    public virtual void RemoveBuff(Buff buff)
    {
        if (buff == null)
            return;

        if (buffs.Remove(buff))
            OnBuffsChanged?.Invoke(Buffs);
    }
    

    public virtual void ClearBuffs()
    {
        if (buffs.Count == 0)
            return;

        buffs.Clear();
        OnBuffsChanged?.Invoke(Buffs);

    }

    //Attack 이벤트 
    public virtual void AttackEvent()
    {
        OnAttack?.Invoke();
        PlaySfx(GetAttackSound());
    }

    public virtual void Damage(Entity target, int damageAmount)
    {
        if (damageAmount < 0)
            return;

        if (_isDead)
            return;
        
        // 방어도가 있으면 방어도를 먼저 깎고, 남은 데미지만 HP에 적용합니다.
        if (curBlock > 0)
        {
            int remainingDamage = damageAmount - curBlock;
            int blockedDamage = Mathf.Min(damageAmount, curBlock);
            curBlock = Mathf.Max(0, curBlock - damageAmount);
            OnBlockChanged?.Invoke(curBlock);

            if (blockedDamage > 0)
                PlaySfx(GetBlockHitSound());

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

        BuffCheck_After(target);

      
        if (curHp <= 0 && !_isDead)
        {
            _isDead = true;
            Die();
        }
    }

    protected virtual void Die()
    {
        OnDead?.Invoke();
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
        if (blockAmount <= 0)
            return;

        curBlock += blockAmount;
        OnBlockChanged?.Invoke(curBlock);
        PlaySfx(GetBlockGainSound());
    }

    public virtual void UseBuff()
    {
        PlaySfx(GetBuffSound());
    }

    public virtual void UseDebuff()
    {
        PlaySfx(GetDebuffSound());
    }

    protected virtual AudioClip GetAttackSound()
    {
        return attackSound;
    }

    protected virtual AudioClip GetBlockGainSound()
    {
        return blockGainSound;
    }

    protected virtual AudioClip GetBlockHitSound()
    {
        return blockHitSound;
    }

    protected virtual AudioClip GetBuffSound()
    {
        return buffSound;
    }

    protected virtual AudioClip GetDebuffSound()
    {
        return debuffSound;
    }

    protected void PlaySfx(AudioClip clip)
    {
        if (clip != null)
            BattleManager.Instance?.PlaySfx(clip);
    }
}
