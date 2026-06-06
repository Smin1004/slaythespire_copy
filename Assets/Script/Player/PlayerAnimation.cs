using UnityEngine;

public class PlayerAnimation : MonoBehaviour
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
    }

    private void OnDisable()
    {
        _entity.OnDamaged -= PlayHitAnimation;
    }

    private void PlayHitAnimation(int damage)
    {
        _animator.SetTrigger("Hit");
    }
}