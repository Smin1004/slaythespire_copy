using System;
using System.Linq;
using UnityEngine;
using UnityEngine.U2D.Animation;

public class EnemyEntity : Entity
{
    [SerializeField] private EnemyData enemyData;
    [SerializeField] private SpriteLibrary spriteLibrary;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer[] spriteRenderers;
    [SerializeField] private bool destroyOnDeath = true;
    [SerializeField] private float deathDestroyDelay = 1f;

    private EnemyAction curAction;
    private bool isSetup;

    public EnemyData CurrentEnemyData => enemyData;
    public EnemyAction CurrentAction => curAction;
    public event Action<EnemyAction> OnIntentChanged;

    private void Awake()
    {
        spriteLibrary = GetComponentInChildren<SpriteLibrary>();
        animator = GetComponentInChildren<Animator>();
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
    }

    private void Start()
    {
        if (!isSetup && enemyData != null)
            SetupEnemy(enemyData);
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

        enemyData = data;
        isSetup = true;

        InitializeEntity(enemyData.maxHealth);
        ApplyPresentation(enemyData);
        DecideNextIntent();
    }

    protected override void InitializeEntity(int startingHealth)
    {
        attackSound = enemyData.attackSound;
        blockGainSound = enemyData.blockGainSound;
        blockHitSound = enemyData.blockHitSound;
        buffSound = enemyData.buffSound;
        debuffSound = enemyData.debuffSound;

        base.InitializeEntity(startingHealth);
    }

    private void ApplyPresentation(EnemyData data)
    {
        if (spriteLibrary != null && data.spriteLibraryAsset != null)
            spriteLibrary.spriteLibraryAsset = data.spriteLibraryAsset;

        if (animator != null && data.animatorController != null)
            animator.runtimeAnimatorController = data.animatorController;
    }

    public void FaceTarget(Transform target)
    {
        if (target == null)
            return;

        if (spriteRenderers == null || spriteRenderers.Length == 0)
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);

        // 보스 스프라이트 기준에 맞춰 플레이어 방향으로 좌우 반전합니다.
        bool shouldFlip = target.position.x > transform.position.x;
        foreach (SpriteRenderer renderer in spriteRenderers)
        {
            if (renderer != null)
                renderer.flipX = shouldFlip;
        }
    }

    public override void TurnInit()
    {
        base.TurnInit();
        //DecideNextIntent();  호출 딴곳에서 여기서하면 공격 표시가 다음공격을 표시해버리는 오류발생
    }

    public void DecideNextIntent()
    {
        if (enemyData == null || enemyData.actions == null || enemyData.actions.Length == 0)
        {
            curAction = null;
            OnIntentChanged?.Invoke(curAction);
            return;
        }

        curAction = PickWeightedAction(enemyData.actions);
        OnIntentChanged?.Invoke(curAction);
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

    public void ExecuteEnemyTurn(Entity target)
    {
        if (IsDead || curAction == null || target == null)
            return;

        Debug.Log($"[{enemyData.enemyName}] Execute action: {curAction.actionName}");
        if (curAction.isAttack) for (int i = 0; i < curAction.hitCount; i++) ExecuteAttack(target, curAction.damage);
        if (curAction.isBlock) ExecuteBlock(curAction.blockAmount);
        if (curAction.isBuffDebuff)
        {
            Buff buff;
            for (int i = 0; i < curAction.buffDebuffs.Length; i++)
            {
                buff = DataManager.Instance.GetBuff(curAction.buffDebuffs[i].id, curAction.buffDebuffs[i].value);
                if (curAction.buffDebuffs[i].isBuffToSelf) AddBuff(buff);
                else target.AddBuff(buff);
            }
        }
        DecideNextIntent();
    }

    private void PlayDamagedFeedback(int appliedDamage)
    {
        if (appliedDamage <= 0)
            return;

        BattleManager.Instance?.SpawnDamageText(appliedDamage, transform);

    }

    protected override void Die()
    {
        base.Die();

        BattleManager.Instance?.RemoveEnemy(this);

        if (destroyOnDeath)
            Destroy(gameObject, deathDestroyDelay);
    }
}
