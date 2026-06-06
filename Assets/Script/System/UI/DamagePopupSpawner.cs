using UnityEngine;
using UnityEngine.UI;

public class DamagePopupSpawner : MonoBehaviour
{
    [SerializeField] private DamageView damagePopupPrefab;
    [SerializeField] private RectTransform canvasParent;
    [SerializeField] private Camera worldCamera;
    [SerializeField] private Camera uiCamera;
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2f, 0f);

    public void SpawnDamageText(Vector3 worldPosition, float damage)
    {
        if (damagePopupPrefab == null)
        {
            Debug.LogWarning("DamagePopupSpawner: damagePopupPrefab가 설정되지 않았습니다.");
            return;
        }

        if (canvasParent == null)
        {
            Debug.LogWarning("DamagePopupSpawner: canvasParent가 설정되지 않았습니다.");
            return;
        }

        DamageView popup = Instantiate(damagePopupPrefab, canvasParent);
        popup.gameObject.SetActive(true);

        Camera cameraToUse = worldCamera != null ? worldCamera : Camera.main;
        Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(cameraToUse, worldPosition + worldOffset);
        RectTransform popupRt = popup.GetComponent<RectTransform>();

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasParent, screenPosition, uiCamera, out Vector2 localPoint))
        {
            popupRt.anchoredPosition = localPoint;
        }
        else
        {
            popupRt.anchoredPosition = Vector2.zero;
        }

        popup.Play(damage);
    }
}
