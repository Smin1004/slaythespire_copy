using UnityEngine;

public class UISound : MonoBehaviour
{
    [SerializeField] soundManager _sfxSource; 
    [SerializeField] AudioClip _hoverSound;
    [SerializeField] AudioClip _clickSound;

    public void PlayHoverSound()
    {
        _sfxSource.PlaySFX(_hoverSound);
    }

    public void PlayClickSound()
    {
        _sfxSource.PlaySFX(_clickSound);
    }
}