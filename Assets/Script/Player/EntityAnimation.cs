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
        _entity.OnDamaged += PlayHitAnimation;
        _entity.OnDead += PlayDeathAnimation;
        _entity.OnRevived += PlayRevive;
        _entity.OnAttack += PlayAttack;
    }

    private void OnDisable()
    {
        _entity.OnDamaged -= PlayHitAnimation;
        _entity.OnDead -= PlayDeathAnimation;
        _entity.OnRevived -= PlayRevive;
        _entity.OnAttack -= PlayAttack;
    }

    private void PlayHitAnimation(int damage)
    {
        _animator.SetTrigger("Hit");
    }

    private void PlayDeathAnimation()
    {
       _animator.SetBool("IsDead", true);
    }

    private void PlayRevive()
    {
        _animator.SetBool("IsDead", false);
    }

    private void PlayAttack()
    {
        _animator.SetTrigger("Attack");
    }

}