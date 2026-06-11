using UnityEngine;
using UnityEngine.UI;

public class BackgroundProvider : MonoBehaviour
{
    [SerializeField] private Sprite backgroundSprite; // 이 화면/패널에서 사용할 배경 Sprite입니다.

    public Sprite BackgroundSprite => backgroundSprite;

    private void Awake()
    {
        if (backgroundSprite != null)
            return;

        // 아직 Sprite를 직접 넣지 않았다면, 기존 Background Image의 sprite를 데이터로 가져옵니다.
        Image[] images = GetComponentsInChildren<Image>(true);
        foreach (Image image in images)
        {
            if (image != null && image.gameObject.name == "Background")
            {
                backgroundSprite = image.sprite;
                break;
            }
        }
    }
}
