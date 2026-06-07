using UnityEngine;

public class EntityAnimation : MonoBehaviour
{
    [SerializeField] private Animator _animator;

    private Entity _entity;

    private void Awake()
    {
        _entity = GetComponent<Entity>();
    }

    private void OnEnable()
    {
        _entity.OnDamaged += PlayHitAnimation;
        _entity.OnDead += PlayDeathAnimation;
        _entity.OnRevived += PlayRevive;
    }

    private void OnDisable()
    {
        _entity.OnDamaged -= PlayHitAnimation;
        _entity.OnDead -= PlayDeathAnimation;
        _entity.OnRevived -= PlayRevive;
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

}