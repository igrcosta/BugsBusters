using UnityEngine;
using TMPro;

public class TimerScript : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI timerText;
    [Header("Inserir o tempo em segundos")]
    [SerializeField] float remainingTime;

    private bool isRunning = false;
    private bool PlayerSafeZoneHit = false;

    //tive que colocar no Awake ao invés do Start, para o sistema de waves já ter referência de forma antecipada
    void Awake()
    {
        GameControllerScript.controller.Timer = this;
        //o timer ao dar start na cena, vai se inserir dentro do GameController
    }

    public void StartTimer()
    {
        isRunning = true;
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    public void Lose10Seconds()
    {
        PlayerSafeZoneHit = true;
    }

    void Update()
    {
        if (isRunning)
        {
            //se for true, roda isso
            Countdown();
        }
        else
        {
            //se for false, roda isso
        }

        if (PlayerSafeZoneHit)
        {
            Reducing10Seconds();
        }
        else
        {
            //NADA
        }
    }

    private void Countdown()
    {
        if(remainingTime > 0)
        {
            remainingTime -= Time.deltaTime;
        }
        else if(remainingTime < 0)
        {
            remainingTime = 0;
            timerText.color = Color.red;
            GameControllerScript.controller.GameOver();
            //quando o tempo zerar, o relógio vai ficar vermelho, e vai executar o método GameOver do gameController
        }
        
        int minutes = Mathf.FloorToInt(remainingTime / 60);
        int seconds = Mathf.FloorToInt(remainingTime % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void Reducing10Seconds()
    {
        remainingTime-=10f;
        PlayerSafeZoneHit = false;
    }
}
