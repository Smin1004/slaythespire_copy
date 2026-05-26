using UnityEngine;


public class soundManager : MonoBehaviour
{
    [SerializeField] AudioSource _bgm;
    [SerializeField] AudioSource _sfx;


    void Start()
    {
        PlayBGM();    
    }

    /// <summary>
    /// 플레이 bgm 재생
    /// </summary> <summary>
    /// 
    /// </summary>
    public void PlayBGM()
    {
        _bgm.Play();
    }

    public void PauseBGM()
    {
        _bgm.Pause();
    }

    public void UnpauseBGM()
    {
        _bgm.UnPause();
    }

    /// <summary>
    /// 
    /// </summary> <summary>
    /// SFX 캐릭터 스왑할 때마다 재생
    /// </summary>
    public void PlaySFX()
    {
        _sfx.Play();
    }


   
}