using UnityEngine;
using UnityEngine.SceneManagement;

public class GameControllerScript : MonoBehaviour
{

    private Player Player;
    //acessar o gameObject do tipo Player

    private bool IsPaused = false;

    public static GameControllerScript controller;
    
    private void Awake()
    {
        if (controller == null)
        {
            controller = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }


    void Update()
    {
        Pause();
    }

    public void Pause()
    {
        if(Input.GetKeyDown(KeyCode.Escape) && IsPaused == false)
        {
            IsPaused = true;
            //aparecer tela de pause com um SetActive
            Time.timeScale = 0;
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && IsPaused == true)
        {
            IsPaused = false;
            //aparecer tela de pause com um SetActive
            Time.timeScale = 1;
        }
    }

    public void GameOver()
    {
        SceneManager.LoadScene(1);
    }

    //GameController vai servir para o seguinte:
      //gerenciar as waves, o que engloba:
        //Controlar o Timer
        //Gerenciar os spawns de inimigos 
        //trazer as condições de vitória e derrota
        //gerenciar quando começa ondas e termina outras
      //trocar entre cenas
      //garantir "pauses" na gameplay (FEITO)
}
