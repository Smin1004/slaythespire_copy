using UnityEngine;

public class Card : PoolableObject
{
    [SerializeField] private float moveSpeed = 10f; // 스르륵 이동하는 속도

    private BattleManager battleManager;
    private Vector3 dragOffset;
    private Player player;
    
    [SerializeField] private Vector3 targetPosition;
    [SerializeField] private Quaternion targetRotation;
    [SerializeField] private bool isCardHeld = false;

    public Skill skill;

    private void Start()
    {
        battleManager = BattleManager.Instance;
        player = Player.Instance;
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
            if (normalizedY >= 0.33f && normalizedY <= 0.66f)
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
        ReturnToHand();
    }

    public bool UseCard(Entity target)
    {
        if (skill == null || player == null)
            return false;

        if (battleManager != null && !battleManager.isPlayerTurn)
            return false;

        if (player.energy < skill.cost)
            return false;

        player.energy -= skill.cost;

        // 별도 SkillScript가 있으면 그 스크립트를 우선 실행합니다.
        if (skill.effect != null)
            skill.effect.Setting(player, target);
        else
            ApplyDefaultSkill(target);

        return true;
    }

    private void ApplyDefaultSkill(Entity target)
    {
        // CSV의 value 첫 번째 값을 기본 공격/방어 수치로 사용합니다.
        int value = skill.skillValue != null && skill.skillValue.Length > 0 ? skill.skillValue[0] : 0;

        switch (skill.type)
        {
            case SkillType.Attack:
                // 공격 카드는 타겟 Entity의 Damage를 호출합니다.
                if (target != null)
                    target.Damage(value);
                break;
            case SkillType.Skill:
                // 스킬 카드는 임시 기본 동작으로 플레이어에게 방어도를 줍니다.
                player.AddBlock(value);
                break;
        }
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
