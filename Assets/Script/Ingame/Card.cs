using TMPro;
using UnityEngine;
using System.Collections;
using System;

public class Card : PoolableObject
{
    [SerializeField] private float moveSpeed = 10f; // Card movement speed.

    private BattleManager battleManager;
    private Vector3 dragOffset;
    private Player player;
    private bool isCardHeld = false;
    private bool isRewardPreview = false;
    private Action<Skill> onRewardSelected;

    public TextMeshPro descText;
    public TextMeshPro costText;
    public TextMeshPro nameText;
    public SpriteRenderer img;
    public Skill skill;
    [SerializeField] private AudioClip useCardSound;

    public Vector3 targetPosition;
    public Quaternion targetRotation;
    bool useCard;

    public void Init(Skill _skill)
    {
        battleManager = BattleManager.Instance;
        player = Player.Instance;

        skill = _skill;
        skill.effect.skillData = skill;
        descText.text = skill.effect.FormatDesc(skill, 0);
        costText.text = skill.cost.ToString();
        nameText.text = skill.name;
        img.sprite = skill.img;
    }

    public void SetRewardPreview(Skill previewSkill, Action<Skill> selectCallback)
    {
        // Reward preview cards show card data and select on click.
        isRewardPreview = true;
        isCardHeld = false;
        onRewardSelected = selectCallback;
        Init(previewSkill);
    }

    public void SetTargetTransform(Vector3 newPosition, Quaternion newRotation)
    {
        targetPosition = newPosition;
        targetRotation = newRotation;
    }

    private void Update()
    {
        if (!isCardHeld)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * moveSpeed);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * moveSpeed);
        }
        else
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.identity, Time.deltaTime * moveSpeed);
        }
    }

    private void OnMouseDown()
    {
        if (isRewardPreview)
            return;

        isCardHeld = true;
        dragOffset = transform.position - GetMouseWorldPosition();
    }

    private void OnMouseDrag()
    {
        if (isRewardPreview || !isCardHeld)
            return;

        transform.position = GetMouseWorldPosition() + dragOffset;
    }

    private void OnMouseUp()
    {
        if (isRewardPreview)
        {
            onRewardSelected?.Invoke(skill);
            return;
        }

        isCardHeld = false;

        if (skill == null)
        {
            ReturnToHand();
            return;
        }

        if (skill.isTargeting)
        {
            // Targeting cards only accept EnemyEntity under the mouse.
            Entity foundTarget = FindEnemyAtMousePosition();

            if (foundTarget != null)
                TriggerCard(foundTarget);
            else
                ReturnToHand();
        }
        else
        {
            // Non-target cards are used when released above the lower hand area.
            float normalizedY = Input.mousePosition.y / Screen.height;
            if (normalizedY >= 0.33f)
                TriggerCard(null);
            else
                ReturnToHand();
        }
    }

    private Entity FindEnemyAtMousePosition()
    {
        Vector2 mouseWorldPosition = GetMouseWorldPosition();
        Collider2D[] hitCollider = Physics2D.OverlapPointAll(mouseWorldPosition);

        foreach (var hit in hitCollider)
        {
            if (hit.CompareTag("Enemy"))
            {
                // Only EnemyEntity can be selected as a card target.
                EnemyEntity enemy = hit.GetComponentInParent<EnemyEntity>();
                if (enemy != null)
                    return enemy;
            }
        }

        return null;
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreenPosition = Input.mousePosition;
        mouseScreenPosition.z = Camera.main.nearClipPlane;
        return Camera.main.ScreenToWorldPoint(mouseScreenPosition);
    }

    private void TriggerCard(Entity target)
    {
        StartCoroutine(UseCard(target));
    }

    public IEnumerator UseCard(Entity target)
    {
        if (skill == null || skill.effect == null || player == null) yield break;
        if (battleManager != null && !battleManager.isPlayerTurn) yield break;
        if (player.energy < skill.cost) yield break;
        if(useCard) yield break;

        useCard = true;
        Entity[] targets;
        if (target != null)
        {
            targets = new Entity[] { target };
        }
        else
        {
            var enemies = BattleManager.Instance != null ? BattleManager.Instance.EnemyList : null;
            targets = enemies != null ? new Entity[enemies.Count] : System.Array.Empty<Entity>();

            if (enemies != null)
            {
                for (int i = 0; i < enemies.Count; i++)
                    targets[i] = enemies[i];
            }
        }

        int[] baseValue = skill.isUpgraded ? skill.upgradeValue : skill.skillValue;
        if (baseValue == null || baseValue.Length == 0)
            yield break;

        // Use a copied value array so card data is not permanently modified.
        int[] value = (int[])baseValue.Clone();

        player.UseEnergy(skill.cost);
        RunManager.Instance?.RecordCardUse(); // 클리어 통계에 사용할 실제 카드 사용 횟수를 기록합니다.
        if (useCardSound != null)
            AudioManager.Instance?.PlaySfx(useCardSound);
        else
            DeckManager.Instance?.PlayUseCardSound();

        targetPosition = Vector3.zero;
        targetRotation = Quaternion.identity;
        value[0] = player.BuffCheck_CardTrigger(skill, value[0]);
        yield return StartCoroutine(skill.effect.Trigger(player, targets, value));
        DeckManager.Instance.DiscardCard(this);
        useCard = false;
    }

    private void ReturnToHand()
    {
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || !isCardHeld || skill == null)
            return;

        if (skill.isTargeting)
        {
            Gizmos.DrawWireSphere(GetMouseWorldPosition(), 0.3f);
            Gizmos.DrawSphere(GetMouseWorldPosition(), 0.05f);
        }
    }
}
