using UnityEngine;

public class EntityAnimation : MonoBehaviour
{
    [SerializeField] private Animator _animator;

    private Entity _entity;

    private void Awake()
    {
        if (_entity == null)
            _entity = GetComponent<Entity>();

        if (_animator == null)
            _animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        if (_entity == null)
            _entity = GetComponent<Entity>();

        if (_entity == null)
            return;

        _entity.OnDamaged += PlayHitAnimation;
        _entity.OnDead += PlayDeathAnimation;
        _entity.OnAttack += PlayAttack;
    }

    private void OnDisable()
    {
        if (_entity == null)
            return;

        _entity.OnDamaged -= PlayHitAnimation;
        _entity.OnDead -= PlayDeathAnimation;
        _entity.OnAttack -= PlayAttack;
    }

    private void PlayHitAnimation(int damage)
    {
        if (_animator != null)
            _animator.SetTrigger("Hit");
    }

    private void PlayDeathAnimation()
    {
        if (_animator != null)
            _animator.SetBool("IsDead", true);
    }

    private void PlayRevive()
    {
        if (_animator != null)
            _animator.SetBool("IsDead", false);
    }

    private void PlayAttack()
    {
        if (_animator == null)
            return;

        string triggerName = "Attack";

        // EnemyAction에 행동별 공격 트리거가 있으면 기본 Attack 대신 그 트리거를 사용합니다.
        if (_entity is EnemyEntity enemy &&
            enemy.CurrentAction != null &&
            !string.IsNullOrEmpty(enemy.CurrentAction.animTrigger))
        {
            triggerName = enemy.CurrentAction.animTrigger;
        }

        _animator.SetTrigger(triggerName);
    }

}
