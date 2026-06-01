using UnityEngine;
using UnityEngine.EventSystems;

public class Card : MonoBehaviour
{
    private BattleManager battleManager;
    private Vector3 originalPosition;
    private Vector3 dragOffset;
    private bool isCardHeld = false;

    private void Start()
    {
        battleManager = BattleManager.Instance;
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

        float normalizedY = Input.mousePosition.y / Screen.height;

        //대략적으로 우선 카드가 중단 ~ 상단에 있으면 사용되게 함
        if (normalizedY >= 0.40f)
        {
            TriggerCardEffect();
        }
        else
        {
            ReturnToHand();
        }
    }

    // 마우스 픽셀 좌표를 월드 좌표로 변환하는 함수
    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreenPosition = Input.mousePosition;
        
        mouseScreenPosition.z = Camera.main.nearClipPlane; 
        
        return Camera.main.ScreenToWorldPoint(mouseScreenPosition);
    }

    private void TriggerCardEffect()
    {
        Debug.Log("카드 사용");
        //임시로 돌아오게 적용

        //임시 카드 사용 로직
        battleManager.enemyList[0].Damage(10);
        ReturnToHand(); 
    }

    private void ReturnToHand()
    {
        transform.position = originalPosition;
    }
}