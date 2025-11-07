using UnityEngine;
using UnityEngine.UI;

public class SongManager : MonoBehaviour
{
    [SerializeField] Slider SongSlider;
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

    private void Load()
    {
        SongSlider.value = PlayerPrefs.GetFloat("musicValue");
    }

    private void Save()
    {
        PlayerPrefs.SetFloat("musicVolume" , SongSlider.value);
    }
}
