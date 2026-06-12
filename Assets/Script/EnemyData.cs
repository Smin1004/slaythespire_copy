using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.U2D.Animation;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "GameData/EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("Basic")]
    public string enemyName;
    // 기존 baseMaxHealth 값을 잃지 않고 새 maxHealth 필드로 이어받습니다.
    [FormerlySerializedAs("baseMaxHealth")]
    public int maxHealth;

    [Header("Reward")]
    // 적 처치 후 보상으로 사용할 데이터입니다. 실제 지급 방식은 RewardManager에서 확장합니다.
    public EnemyRewardData reward;

    [Header("Actions")]
    // 기존 actionList 값을 잃지 않고 새 actions 배열로 이어받습니다.
    [FormerlySerializedAs("actionList")]
    public EnemyAction[] actions;

    [Header("Presentation")]
    // 하나의 Enemy 프리팹에 적별 SpriteLibrary를 갈아끼우기 위한 외형 데이터입니다.
    public SpriteLibraryAsset spriteLibraryAsset;
    // 적 종류별 애니메이션 컨트롤러를 EnemyData에서 지정합니다.
    public RuntimeAnimatorController animatorController;

    [Header("Sounds")]
    // 공통 피드백은 BattleManager를 통해 재생하고, 실제 클립은 EnemyData가 들고 있습니다.
    // hitSound는 플레이어 피격 쪽에서 사용할 예정이라 적 데이터에서는 일단 사용하지 않습니다.
    // public AudioClip hitSound;
    public AudioClip attackSound;
    public AudioClip blockGainSound;
    public AudioClip blockHitSound;
    public AudioClip buffSound;
    public AudioClip debuffSound;
    // deathSound는 현재 사용하지 않으므로 일단 주석 처리합니다.
    // public AudioClip deathSound;

    // [Header("Effects")]
    // 피격/공격 이펙트 프리팹입니다. 생성 위치와 재생은 BattleManager/EffectManager가 담당합니다.
    // 이펙트 구조는 아직 확정 전이라 EnemyData에서는 잠시 사용하지 않습니다.
    // public GameObject hitEffect;
    // public GameObject attackEffect;
}

[System.Serializable]
public class EnemyRewardData
{
    public int minGold = 10;
    public int maxGold = 20;
    public Skill[] cardRewardPool;
}

public enum IntentType
{
    Attack,
    Defend,
    Buff,
    Debuff
}

public enum BuffDebuffType
{
    None,
    Strength,
    Weak,
    Vulnerable,
    Frail,
    Dexterity
}

[System.Serializable] 
public class EnemyAction
{
    public string actionName;
    public int weight;

    [Header("공격")]
    public bool isAttack;
    public int damage;
    public int hitCount;

    [Header("방어")]
    public bool isBlock;
    public int blockAmount;

    [Header("상태 이상")]
    public bool isBuffDebuff;
    public BuffDebuffAction[] buffDebuffs;

    [Header("행동")]
    public string animTrigger = "Attack";
    public AudioClip actionSound;
    public Sprite intentIcon;
}

[System.Serializable]
public class BuffDebuffAction
{
    public int id;
    public int value;
    public bool isDebuff;
    public bool isBuffToSelf;
}