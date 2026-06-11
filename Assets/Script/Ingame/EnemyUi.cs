using UnityEngine;

/// 역할:
/// - 적 의도 UI
/// - 적 블록 UI
/// - 적 체력바
/// - 적 상태이상 UI까지 한 번에 연결
/// </summary>
public class EnemyUI : MonoBehaviour
{
    [SerializeField] private EnemyIntentView intentView; // 적의 다음 행동 표시 UI입니다.
    [SerializeField] private EntityBlock blockView;      // 적의 블록 수치 표시 UI입니다.
    [SerializeField] private HpBar hpBar;                // 적의 현재 HP / 최대 HP를 표시하는 UI입니다.
    [SerializeField] private StatusEffectBar statusEffectBar; // 적에게 걸린 버프/디버프 아이콘을 표시하는 UI입니다.
    [SerializeField] private StatusEffectTooltip statusTooltip; // 상태이상 아이콘에 마우스를 올렸을 때 보여줄 설명창입니다.

    private void Awake()
    {
        // 프리팹에서 직접 할당하지 않아도 자식 오브젝트에 있는 UI 컴포넌트를 찾아 연결합니다.
        if (intentView == null)
            intentView = GetComponentInChildren<EnemyIntentView>(true);

        if (blockView == null)
            blockView = GetComponentInChildren<EntityBlock>(true);

        if (hpBar == null)
            hpBar = GetComponentInChildren<HpBar>(true);

        if (statusEffectBar == null)
            statusEffectBar = GetComponentInChildren<StatusEffectBar>(true);

        if (statusTooltip == null)
            statusTooltip = GetComponentInChildren<StatusEffectTooltip>(true);
    }

    /// <summary>
    /// EnemyUI를 특정 적과 연결합니다.
    /// 
    /// 호출 위치:
    /// - BattleManager.SpawnEnemies()에서 적을 생성한 직후
    /// </summary>
    public void Bind(EnemyEntity enemy)
    {
        if (intentView != null)
            intentView.Bind(enemy);

        if (blockView != null)
            blockView.Bind(enemy);

        if (hpBar != null)
            hpBar.SetTarget(enemy);

        // HpBar의 자식이 아닌 별도 상태이상 UI도 같은 적 데이터를 보도록 연결합니다.
        if (statusEffectBar != null)
        {
            // 상태이상 아이콘이 생성될 때 같은 툴팁을 사용하도록 설명창을 먼저 전달합니다.
            statusEffectBar.SetTooltip(statusTooltip);
            statusEffectBar.SetTarget(enemy);
        }
    }
}
