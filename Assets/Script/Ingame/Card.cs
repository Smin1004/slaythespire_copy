using TMPro;
using UnityEngine;
using System.Collections;

public class Card : PoolableObject
{
    [SerializeField] private float moveSpeed = 10f; // 스르륵 이동하는 속도

    private BattleManager battleManager;
    private Vector3 dragOffset;
    private Player player;
    private bool isCardHeld = false;
    
    public TextMeshPro descText;
    public TextMeshPro costText;
    public TextMeshPro nameText;
    public SpriteRenderer img;
    public Skill skill;
    [SerializeField] private AudioClip useCardSound;

    public Vector3 targetPosition;
    public Quaternion targetRotation;
    
    public void Init(Skill _skill)
    {
        battleManager = BattleManager.Instance;
        player = Player.Instance;

        skill = _skill;
        descText.text = skill.desc;
        costText.text = skill.cost.ToString();
        nameText.text = skill.name;
        img.sprite = skill.img;
    }
    
    public void SetTargetTransform(Vector3 newPosition, Quaternion newRotation)
    {
        targetPosition = newPosition;
        targetRotation = newRotation;
    }

    private void Update()
    {
        // 마우스로 잡고 있지 않을 때만 목표를 향해 부드럽게 이동 (Target Chasing)
        if (!isCardHeld)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * moveSpeed);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * moveSpeed);
        }else transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.identity, Time.deltaTime * moveSpeed);
    }

    private void OnMouseDown()
    {
        isCardHeld = true;
        dragOffset = transform.position - GetMouseWorldPosition();
    }

    private void OnMouseDrag()
    {
        if (!isCardHeld)
            return;

        transform.position = GetMouseWorldPosition() + dragOffset;
    }

    private void OnMouseUp()
    {
        isCardHeld = false;

        if (skill == null)
        {
            ReturnToHand();
            return;
        }

        if (skill.isTargeting)
        {
            // 타겟팅 카드면 마우스 위치 아래의 Enemy 태그 오브젝트를 찾습니다.
            Entity foundTarget = FindEnemyAtMousePosition();
            Debug.Log(foundTarget == null);

                if (foundTarget != null)
                TriggerCard(foundTarget);
            else
                ReturnToHand();
        }
        else
        {
            // 타겟이 없는 카드는 화면 중앙 영역에 놓았을 때 사용됩니다.
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
            Debug.Log(hit.tag);
            if (hit.CompareTag("Enemy"))
            {
                return hit.GetComponentInParent<Entity>();
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
        // 카드 효과를 적용한 뒤 원래 손패 위치로 되돌립니다.
        Debug.Log("사용");
        StartCoroutine(UseCard(target));
    }

    public IEnumerator UseCard(Entity target)
    {
        if (skill == null || player == null)
            yield return false;

        if (battleManager != null && !battleManager.isPlayerTurn)
            yield return false;

        if (player.energy < skill.cost)
            yield return false;

        Entity[] targets;
        if (target != null)
            targets = new Entity[] { target };
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

        player.UseEnergy(skill.cost);
        if (useCardSound != null)
            BattleManager.Instance?.PlaySfx(useCardSound);
        else
            DeckManager.Instance?.PlayUseCardSound();
        targetPosition = Vector3.zero;
        targetRotation = Quaternion.identity;
        yield return StartCoroutine(skill.effect.Trigger(player, targets, skill.skillValue));
        DeckManager.Instance.DiscardCard(this);
    }

    private void ReturnToHand()
    {
       Debug.Log("복귀");
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
