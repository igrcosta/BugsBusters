using UnityEngine;
using UnityEngine.UI;

public class SongManager : MonoBehaviour
{
    [SerializeField] Slider SongSlider;
    [SerializeField] Slider SFXSlider;
    void Start()
    {
        if (PlayerPrefs.HasKey("musicValue"))
        {
            PlayerPrefs.SetFloat("musicValue", 1);
            Load();
        }
        else
        {
            Load();
        }
    }

    
    public void ChangeVolume()
    {
        AudioListener.volume = SongSlider.value;
        Save();
    }



    public void ChangeSFXVolume()
    {
        AudioListener.volume = SFXSlider.value;
        Save();
    }

    private void Load()
    {
        SongSlider.value = PlayerPrefs.GetFloat("musicValue");
    }

    private void Save()
    {
        PlayerPrefs.SetFloat("musicVolume" , SongSlider.value);
    }
}
