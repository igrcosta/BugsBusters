using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SongManager : MonoBehaviour
{
    public AudioMixer mixer;
    public Slider sfxVol;
    public Slider musicVol;
    public Slider masterVol;

    // Constante para mapear o volume de 0 a 1 para dB
    private const float LogarithmicMultiplier = 20f;
    // O valor mínimo do slider para evitar log(0). 
    // Deve corresponder ao 'Min Value' do Slider UI.
    private const float MinSliderValue = 0.0001f; 

    private float ConvertToDecibels(float sliderValue)
    {
        // 1. Garante que o valor não é zero, evitando Log10(0)
        float clampedValue = Mathf.Max(sliderValue, MinSliderValue);
        
        // 2. Converte o valor linear (0 a 1) para a escala logarítmica (dB).
        // A multiplicação por 20 é o padrão para conversão de volume.
        return Mathf.Log10(clampedValue) * LogarithmicMultiplier;
    }

    public void MusicVolChange()
    {
        float dbValue = ConvertToDecibels(musicVol.value);
        mixer.SetFloat("MusicVol", dbValue);
    }

    public void SFXVolChange()
    {
        float dbValue = ConvertToDecibels(sfxVol.value);
        mixer.SetFloat("SFXVol", dbValue);
    }

    public void MasterVolChange()
    {
        float dbValue = ConvertToDecibels(masterVol.value);
        mixer.SetFloat("MasterVol", dbValue);
    }

    //TIVE QUE FAZER UM CÁLCULO CHATO DE DECIBÉIS PRA FICAR GOSTOSINHO O SOM DO GAME

    void Start()
    {
        masterVol.value = 0.5f;
        musicVol.value = 0.5f;
        sfxVol.value = 0.5f;
        
        MasterVolChange();
        SFXVolChange();
        MusicVolChange();
    }
}