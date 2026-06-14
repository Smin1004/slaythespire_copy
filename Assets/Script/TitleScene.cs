using UnityEngine;

public class TitleScene : MonoBehaviour
{


    public void TitleScenes()
    {
        // Start through the fade manager so the title screen fades out first.
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.LoadSceneWithFade("GamePlay");
    }
}
