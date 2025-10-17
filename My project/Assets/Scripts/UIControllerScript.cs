using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class UIControllerScript : MonoBehaviour
{
    [SerializeField] Text InimigosMortos;
    public void Retry()
    {
        SceneManager.LoadScene(1);
    }

    private void Start()
    {
        GameControllerScript.controller.UIController = this;
    }

    public void AlterarInimigosMortosnaHUD(int quantosinInimigosMortos)
    {
        InimigosMortos.text = "Inimiogs Mortos: " + quantosinInimigosMortos;
    }
}
