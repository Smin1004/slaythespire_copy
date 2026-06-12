using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyIntentView : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private EnemyEntity enemy;

    [Header("UI")]
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text valueText;

    [Header("아이콘")]
    [SerializeField] private Sprite attackIcon;
    [SerializeField] private Sprite defendIcon;
    [SerializeField] private Sprite buffIcon;
    [SerializeField] private Sprite debuffIcon;

    private void OnEnable()
    {
        if (enemy != null)
            enemy.OnIntentChanged += UpdateIntent;
    }

    private void OnDisable()
    {
        if (enemy != null)
            enemy.OnIntentChanged -= UpdateIntent;
    }

    private void Start()
    {
        if (enemy != null)
            UpdateIntent(enemy.CurrentAction);
    }

    private void UpdateIntent(EnemyAction action)
    {
        if (action == null)
            return;

        if (action.intentIcon != null)
            icon.sprite = action.intentIcon;

        if(action.isAttack)
        {
            int checkDamage = enemy.ExecuteAttack(Player.Instance, action.damage, true);
            checkDamage = Player.Instance.ExecuteBlock(checkDamage, true);

            // 연타 여부에 따라 텍스트 포맷 결정
            if (action.hitCount > 1) valueText.text = $"{checkDamage} x {action.hitCount}";
            else valueText.text = $"{checkDamage}";
                
            icon.sprite = attackIcon;
        }
        if (action.isBlock)
        {
            int checkBlock = enemy.ExecuteBlock(action.blockAmount, true);
            valueText.text = $"{checkBlock}";
                
            icon.sprite = attackIcon;
        }
        if (action.isBuffDebuff)
        {
            foreach(var n in action.buffDebuffs)
            {
                if (n.isBuffToSelf) icon.sprite = buffIcon;
                else icon.sprite = debuffIcon;
            }
            valueText.text = "";
        }
    }

    /// - 적 프리팹이 씬 Canvas UI를 직접 참조하지 않게 하기 위해서입니다.
    /// </summary>
    public void Bind(EnemyEntity _enemy)
    {
        if (enemy != null)
            enemy.OnIntentChanged -= UpdateIntent;

        enemy = _enemy;

        if (enemy != null)
        {
            enemy.OnIntentChanged += UpdateIntent;
            UpdateIntent(enemy.CurrentAction);
        }
    }
}
