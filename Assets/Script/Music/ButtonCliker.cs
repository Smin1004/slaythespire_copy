using UnityEngine;

public class UISound : MonoBehaviour
{
    [SerializeField] AudioManager _sfxSource; 
    [SerializeField] AudioClip _hoverSound;
    [SerializeField] AudioClip _clickSound;

    public void PlayHoverSound()
    {
        _sfxSource.PlaySfx(_hoverSound);
    }

    public void PlayClickSound()
    {
        _sfxSource.PlaySfx(_clickSound);
    }
}