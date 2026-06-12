using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyIntentView : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private EnemyEntity _enemy;

    [Header("UI")]
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _valueText;

    [Header("아이콘")]
    [SerializeField] private Sprite _attackIcon;
    [SerializeField] private Sprite _defendIcon;
    [SerializeField] private Sprite _buffIcon;
    [SerializeField] private Sprite _debuffIcon;

    private void OnEnable()
    {
        if (_enemy != null)
            _enemy.OnIntentChanged += UpdateIntent;
    }

    private void OnDisable()
    {
        if (_enemy != null)
            _enemy.OnIntentChanged -= UpdateIntent;
    }

    private void Start()
    {
        if (_enemy != null)
            UpdateIntent(_enemy.CurrentAction);
    }

    private void UpdateIntent(EnemyAction action)
    {
        if (action == null)
            return;

        if (action.intentIcon != null)
            _icon.sprite = action.intentIcon;

        switch (action.intentType)
        {
            case IntentType.Attack:

                if (action.intentIcon == null)
                    _icon.sprite = _attackIcon;
                _valueText.text = action.attackDamage.ToString();

                break;

            case IntentType.Defend:

                if (action.intentIcon == null)
                    _icon.sprite = _defendIcon;
                _valueText.text = action.blockAmount.ToString();

                break;

            case IntentType.Buff:

                if (action.intentIcon == null)
                    _icon.sprite = _buffIcon;
                _valueText.text = "";

                break;

            case IntentType.Debuff:

                if (action.intentIcon == null)
                    _icon.sprite = _debuffIcon;
                _valueText.text = "";

                break;
        }
    }

    /// - 적 프리팹이 씬 Canvas UI를 직접 참조하지 않게 하기 위해서입니다.
    /// </summary>
    public void Bind(EnemyEntity enemy)
    {
        if (_enemy != null)
            _enemy.OnIntentChanged -= UpdateIntent;

        _enemy = enemy;

        if (_enemy != null)
        {
            _enemy.OnIntentChanged += UpdateIntent;
            UpdateIntent(_enemy.CurrentAction);
        }
    }
}
