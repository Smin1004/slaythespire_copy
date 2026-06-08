using TMPro;
using UnityEngine;

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

    public Vector3 targetPosition;
    public Quaternion targetRotation;
    
    public void Init(Skill _skill)
    {
        battleManager = BattleManager.Instance;
        player = Player.Instance;

        skill = _skill;
        descText.text = skill.skill_desc;
        costText.text = skill.cost.ToString();
        nameText.text = skill.name;
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
        }
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
            if (hit.CompareTag("Enemy"))
                return hit.GetComponent<Entity>();
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
        UseCard(target);
        DeckManager.Instance.DiscardCard(this);
    }

    public bool UseCard(Entity target)
    {
        if (skill == null || player == null)
            return false;

        if (battleManager != null && !battleManager.isPlayerTurn)
            return false;

        if (player.energy < skill.cost)
            return false;

        Entity[] targets;
        if (target != null)
            targets = new Entity[] { target };
        else
            targets = BattleManager.Instance.enemyList.ToArray();

        player.energy -= skill.cost;
        skill.effect.Trigger(player, targets, skill.skillValue);

        return true;
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
