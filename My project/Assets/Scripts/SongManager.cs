using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SongManager : MonoBehaviour
{
    public AudioMixer mixer;
    public Slider sfxVol;
    public Slider musicVol;
    public Slider masterVol;

    private void Start()
    {
        
    }

    public void MusicVolChange()
    {
        mixer.SetFloat("MusicVol", musicVol.value);
    }

    public void SFXVolChange()
    {
        mixer.SetFloat("SFXVol", sfxVol.value);
    }

    public void MasterVolChange()
    {
        mixer.SetFloat("MasterVol", masterVol.value);
    }
}
