using System;
using UnityEngine;
using UnityEngine.U2D.Animation;

public class EnemyEntity : Entity
{
    // EnemyData는 이 적의 종류를 정의하고, EnemyEntity는 런타임 상태만 관리합니다.
    [SerializeField] private EnemyData currentEnemyData;
    // 프리팹에 붙은 SpriteLibrary에 EnemyData의 SpriteLibraryAsset을 적용합니다.
    [SerializeField] private SpriteLibrary spriteLibrary;
    // 프리팹에 붙은 Animator에 EnemyData의 RuntimeAnimatorController를 적용합니다.
    [SerializeField] private Animator animator;
    // 사망 이벤트/애니메이션이 실행될 시간을 주기 위해 즉시 삭제하지 않을 수 있습니다.
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
<<<<<<< HEAD
        if (!isSetup && currentEnemyData != null)
            SetupEnemy(currentEnemyData);
    }

    private void OnEnable()
    {
        // Entity의 OnDamaged 이벤트를 이용해 피격 공통 연출을 BattleManager로 위임합니다.
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

        // 체력 같은 런타임 값은 데이터에서 읽어오되, 실제 현재 상태는 Entity가 보관합니다.
        InitializeEntity(currentEnemyData.maxHealth);
        ApplyPresentation(currentEnemyData);
        DecideNextIntent();
    }

    private void ApplyPresentation(EnemyData data)
    {
        // 하나의 Enemy 프리팹을 유지하고, SpriteLibraryAsset만 교체해서 외형을 바꿉니다.
        if (spriteLibrary != null && data.spriteLibraryAsset != null)
            spriteLibrary.spriteLibraryAsset = data.spriteLibraryAsset;

        // 적별 애니메이션 세트도 EnemyData에서 지정된 컨트롤러로 교체합니다.
        if (animator != null && data.animatorController != null)
            animator.runtimeAnimatorController = data.animatorController;
    }

    public void DecideNextIntent()
    {
        // 행동 목록이 비어 있으면 의도 UI에도 null을 알려서 잘못된 표시를 막습니다.
        if (currentEnemyData == null || currentEnemyData.actions == null || currentEnemyData.actions.Length == 0)
        {
            currentAction = null;
            OnIntentChanged?.Invoke(currentAction);
            return;
        }
=======
        curEnemyData = assignedData;
        InitializeEntity(curEnemyData.baseMaxHealth);
        
        TurnInit();
    }

    public override void TurnInit()
    {
        base.TurnInit();
        
        //우선은 랜덤값
        //int randomIndex = Random.Range(0, curEnemyData.actionList.Count);
        int randomIndex = UnityEngine.Random.Range(0, curEnemyData.actionList.Count);
>>>>>>> origin/main

        currentAction = PickWeightedAction(currentEnemyData.actions);
        OnIntentChanged?.Invoke(currentAction);
    }

    private EnemyAction PickWeightedAction(EnemyAction[] actions)
    {
        // EnemyAction.weight 값을 기준으로 다음 행동을 가중 랜덤 선택합니다.
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
                Debug.Log($"Buff: {currentAction.buffDebuffType} {currentAction.effectValue}");
                break;
            case IntentType.Debuff:
                UseDebuff();
                Debug.Log($"Debuff: {currentAction.buffDebuffType} {currentAction.effectValue}");
                break;
        }
<<<<<<< HEAD

        DecideNextIntent();
=======
        
        //행동끝 다음행동 시작
        TurnInit();
>>>>>>> origin/main
    }

    private void ExecuteAttack(Entity playerTarget)
    {
        Attack();

        // 행동 전용 리소스가 있으면 우선 사용하고, 없으면 EnemyData의 기본 공격 리소스를 사용합니다.
        // 이펙트는 아직 구조를 확정하지 않았으므로 잠시 사용하지 않습니다.
        // GameObject effect = currentAction.actionEffect != null
        //     ? currentAction.actionEffect
        //     : currentEnemyData.attackEffect;
        // BattleManager.Instance?.PlayEffect(effect, transform);

        int hits = Mathf.Max(1, currentAction.attackCount);
        for (int i = 0; i < hits; i++)
            playerTarget.Damage(currentAction.attackDamage);
    }

    public void TakeDamage(int damageAmount)
    {
        // 외부에서 EnemyEntity 의미로 호출할 수 있도록 남긴 래퍼입니다.
        Damage(damageAmount);
    }

    private void PlayDamagedFeedback(int appliedDamage)
    {
        if (appliedDamage <= 0)
            return;

        // 데미지 텍스트, 카메라, 사운드, 이펙트는 모두 BattleManager 경유로 처리합니다.
        BattleManager.Instance?.SpawnDamageText(appliedDamage, transform);
        BattleManager.Instance?.ShakeCamera();
        // hitSound는 플레이어 피격 쪽에서 사용할 예정이라 적 피격음으로는 재생하지 않습니다.
        // BattleManager.Instance?.PlaySfx(currentEnemyData != null ? currentEnemyData.hitSound : null);
        // 이펙트는 아직 구조를 확정하지 않았으므로 잠시 사용하지 않습니다.
        // BattleManager.Instance?.PlayEffect(currentEnemyData != null ? currentEnemyData.hitEffect : null, transform);
    }

    protected override void Die()
    {
        // Entity의 OnDead 이벤트를 먼저 호출해서 애니메이션/UI 구독자가 반응할 수 있게 합니다.
        base.Die();

        // deathSound는 현재 사용하지 않으므로 재생하지 않습니다.
        // BattleManager.Instance?.PlaySfx(currentEnemyData != null ? currentEnemyData.deathSound : null);
        // 전투 승리 판정은 BattleManager가 enemyList를 기준으로 처리합니다.
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
