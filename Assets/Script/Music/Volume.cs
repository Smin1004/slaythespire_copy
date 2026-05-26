using UnityEngine;
using UnityEngine.Audio;

public class Volume : MonoBehaviour
{
     [SerializeField] AudioMixer _bgmMixer;

     public void BGMSoundVoume(float val)
    {
        _bgmMixer.SetFloat("BGM", Mathf.Log10(val) * 20);
      
    }
    
    public void SFXSoundVoume(float val)
    {
        _bgmMixer.SetFloat("SFX", Mathf.Log10(val) * 20);
    }

    public void SetBGM(bool isOn)
    {
        if (isOn)
        {
            _bgmMixer.SetFloat("BGM", 0f);      // 소리 ON
        }
        else
        {
            _bgmMixer.SetFloat("BGM", -80f);    // 소리 OFF (완전 음소거)
        }
    }
}
