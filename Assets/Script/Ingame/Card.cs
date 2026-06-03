using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class Card : PoolableObject
{
    private BattleManager battleManager;
    private Vector3 originalPosition;
    private Vector3 dragOffset;
    private Player player;
    private bool isCardHeld = false;

    public Skill skill;

    private void Start()
    {
        battleManager = BattleManager.Instance;
        player = Player.Instance;
        //돌아올 위치 임시 초기화
        originalPosition = transform.position;
    }

    // 클릭 시
    private void OnMouseDown()
    {
        isCardHeld = true;

        dragOffset = transform.position - GetMouseWorldPosition();
    }

    // 드래그
    private void OnMouseDrag()
    {
        if (!isCardHeld) return;

        transform.position = GetMouseWorldPosition() + dragOffset;
    }

    // 뗐을때
    private void OnMouseUp()
    {
        isCardHeld = false;

        if (skill.isTargeting)
        {
            // 단일 타겟 카드: 마우스를 놓은 곳에 적이 있는지 검사
            Entity foundTarget = FindEnemyAtMousePosition();

            if (foundTarget != null)
            {
                TriggerCard(foundTarget);
            }
            else
            {
                Debug.Log("타겟팅 실패");
                ReturnToHand();
            }
        }
        else
        {
            float normalizedY = Input.mousePosition.y / Screen.height;
            if (normalizedY >= 0.33f && normalizedY <= 0.66f)
            {
                TriggerCard(null);
            }
            else
            {
                ReturnToHand();
            }
        }
    }

    private Entity FindEnemyAtMousePosition()
    {
        Vector2 mouseWorldPosition = GetMouseWorldPosition();
        Collider2D[] hitCollider = Physics2D.OverlapPointAll(mouseWorldPosition);
        Debug.Log(mouseWorldPosition);
        foreach (var hit in hitCollider)
        {
            if (hit.CompareTag("Enemy"))
            {
                Entity enemyEntity = hit.GetComponent<Entity>();
                return enemyEntity;
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
        //임시로 돌아오게 만든것 / 사용시 발동되는 로직이니 나중에 사용카드 목록으로 바꿔야함
        ReturnToHand();
    }

    public void UseCard()
    {
        if (player.energy < skill.cost) return;

    }

    private void ReturnToHand()
    {
        transform.position = originalPosition;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || !isCardHeld) return;

        if (skill.isTargeting)
        {
            Gizmos.DrawWireSphere(GetMouseWorldPosition(), 0.3f);

            Gizmos.DrawSphere(GetMouseWorldPosition(), 0.05f);
        }
    }
}