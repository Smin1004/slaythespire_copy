using System;
using UnityEngine;
using UnityEngine.U2D.Animation;

public class EnemyEntity : Entity
{
    [SerializeField] private EnemyData currentEnemyData;
    [SerializeField] private SpriteLibrary spriteLibrary;
    [SerializeField] private Animator animator;
    [SerializeField] private bool destroyOnDeath = true;
    [SerializeField] private float deathDestroyDelay = 1f;

    private EnemyAction currentAction;
    private bool isSetup;

    public EnemyData CurrentEnemyData => currentEnemyData;
    public EnemyAction CurrentAction => currentAction;
    public event Action<EnemyAction> OnIntentChanged;

    private void Awake()
    {
        if (spriteLibrary == null)
            spriteLibrary = GetComponentInChildren<SpriteLibrary>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        if (!isSetup && currentEnemyData != null)
            SetupEnemy(currentEnemyData);
    }

    private void OnEnable()
    {
        OnDamaged += PlayDamagedFeedback;
    }

    private void OnDisable()
    {
        OnDamaged -= PlayDamagedFeedback;
    }

    public void SetupEnemy(EnemyData data)
    {
        if (data == null)
        {
            Debug.LogWarning($"{nameof(EnemyEntity)} setup failed: EnemyData is null.", this);
            return;
        }

        currentEnemyData = data;
        isSetup = true;

        InitializeEntity(currentEnemyData.maxHealth);
        ApplyPresentation(currentEnemyData);
        DecideNextIntent();
    }

    private void ApplyPresentation(EnemyData data)
    {
        if (spriteLibrary != null && data.spriteLibraryAsset != null)
            spriteLibrary.spriteLibraryAsset = data.spriteLibraryAsset;

        if (animator != null && data.animatorController != null)
            animator.runtimeAnimatorController = data.animatorController;
    }

    public override void TurnInit()
    {
        base.TurnInit();
        DecideNextIntent();
    }

    public void DecideNextIntent()
    {
        if (currentEnemyData == null || currentEnemyData.actions == null || currentEnemyData.actions.Length == 0)
        {
            currentAction = null;
            OnIntentChanged?.Invoke(currentAction);
            return;
        }

        currentAction = PickWeightedAction(currentEnemyData.actions);
        OnIntentChanged?.Invoke(currentAction);
    }

    private EnemyAction PickWeightedAction(EnemyAction[] actions)
    {
        int totalWeight = 0;
        foreach (EnemyAction action in actions)
        {
            if (action != null)
                totalWeight += Mathf.Max(0, action.weight);
        }

        if (totalWeight <= 0)
            return actions[UnityEngine.Random.Range(0, actions.Length)];

        int roll = UnityEngine.Random.Range(0, totalWeight);
        foreach (EnemyAction action in actions)
        {
            if (action == null)
                continue;

            roll -= Mathf.Max(0, action.weight);
            if (roll < 0)
                return action;
        }

        return actions[0];
    }

    public void ExecuteEnemyTurn(Entity playerTarget)
    {
        if (IsDead || currentAction == null || playerTarget == null)
            return;

        Debug.Log($"[{currentEnemyData.enemyName}] Execute action: {currentAction.actionName}");

        switch (currentAction.intentType)
        {
            case IntentType.Attack:
                ExecuteAttack(playerTarget);
                break;
            case IntentType.Defend:
                AddBlock(currentAction.blockAmount);
                break;
            case IntentType.Buff:
                UseBuff();
                ApplyStatus(this);
                Debug.Log($"Buff: {currentAction.buffDebuffType} {currentAction.effectValue}");
                break;
            case IntentType.Debuff:
                UseDebuff();
                ApplyStatus(playerTarget);
                Debug.Log($"Debuff: {currentAction.buffDebuffType} {currentAction.effectValue}");
                break;
        }

        DecideNextIntent();
    }

    private void ExecuteAttack(Entity playerTarget)
    {
        Attack();

        int hits = Mathf.Max(1, currentAction.attackCount);
        for (int i = 0; i < hits; i++)
            playerTarget.Damage(currentAction.attackDamage);
    }

    private void ApplyStatus(Entity target)
    {
        if (target == null || currentAction == null || currentAction.buffDebuffType == BuffDebuffType.None)
            return;

        LoadData loadData = DataManager.Instance != null ? DataManager.Instance.loadData : null;
        if (loadData == null)
            return;

        Buff template = loadData.GetBuffByKeyOrName(currentAction.statusKeyOrName);
        if (template == null)
            template = loadData.GetBuffByKey(currentAction.buffDebuffType.ToString().ToLowerInvariant());

        if (template == null)
            return;

        Buff runtimeBuff = new Buff
        {
            index = template.index,
            name = template.name,
            key = template.key,
            isDebuff = template.isDebuff,
            value = currentAction.effectValue,
            img = template.img,
            effect = template.effect,
            desc = template.desc
        };

        target.AddBuff(runtimeBuff);
    }

    public void TakeDamage(int damageAmount)
    {
        Damage(damageAmount);
    }

    private void PlayDamagedFeedback(int appliedDamage)
    {
        if (appliedDamage <= 0)
            return;

        BattleManager.Instance?.SpawnDamageText(appliedDamage, transform);
        BattleManager.Instance?.ShakeCamera();
    }

    protected override void Die()
    {
        base.Die();

        BattleManager.Instance?.RemoveEnemy(this);

        if (destroyOnDeath)
            Destroy(gameObject, deathDestroyDelay);
    }

    protected override AudioClip GetAttackSound()
    {
        if (currentAction != null && currentAction.actionSound != null)
            return currentAction.actionSound;

        if (currentEnemyData != null && currentEnemyData.attackSound != null)
            return currentEnemyData.attackSound;

        return base.GetAttackSound();
    }

    protected override AudioClip GetBlockGainSound()
    {
        if (currentEnemyData != null && currentEnemyData.blockGainSound != null)
            return currentEnemyData.blockGainSound;

        return base.GetBlockGainSound();
    }

    protected override AudioClip GetBlockHitSound()
    {
        if (currentEnemyData != null && currentEnemyData.blockHitSound != null)
            return currentEnemyData.blockHitSound;

        return base.GetBlockHitSound();
    }

    protected override AudioClip GetBuffSound()
    {
        if (currentAction != null && currentAction.actionSound != null)
            return currentAction.actionSound;

        if (currentEnemyData != null && currentEnemyData.buffSound != null)
            return currentEnemyData.buffSound;

        return base.GetBuffSound();
    }

    protected override AudioClip GetDebuffSound()
    {
        if (currentAction != null && currentAction.actionSound != null)
            return currentAction.actionSound;

        if (currentEnemyData != null && currentEnemyData.debuffSound != null)
            return currentEnemyData.debuffSound;

        return base.GetDebuffSound();
    }
}
