using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class Card : MonoBehaviour
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
                Debug.Log("[Targeting] 적을 타겟팅하지 않았습니다. 카드가 돌아갑니다.");
                ReturnToHand();
            }
        }
        else // CardTargetType.None (타겟 불필요 카드)
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
        if (target != null)
        {
            Debug.Log($"[Action] {target.name}을(를) 타겟으로 카드 사용!");
        }
        else
        {
            Debug.Log("[Action] 허공(논타겟)에 카드 사용!");
        }

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
        // 게임이 실행 중이고, 카드를 쥐고 있을 때만 기즈모를 그립니다.
        if (!Application.isPlaying || !isCardHeld) return;

        // 단일 타겟팅 카드인 경우에만 레이캐스트 지점을 표시합니다.
        if (skill.isTargeting)
        {


            // 마우스 위치에 반지름 0.3f 짜리 와이어 스피어(빈 원)를 그립니다.
            Gizmos.DrawWireSphere(GetMouseWorldPosition(), 0.3f);

            // 중심점에 작은 십자가나 점을 추가로 그리면 더 직관적입니다.
            Gizmos.DrawSphere(GetMouseWorldPosition(), 0.05f);
        }
    }
}