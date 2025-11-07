using UnityEngine;
using UnityEngine.SceneManagement;

public class CanvasController : MonoBehaviour
{
    public void Retry()
    {
        SceneManager.LoadScene(1);
    }

    public void ExitGame()
    {
        Debug.Log("Saindo do jogo...");

        // Fecha o aplicativo quando estiver buildado
        Application.Quit();

        // Se estiver rodando no editor, para o modo Play
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void MenuGame()
    {
        SceneManager.LoadScene(0);
    }
}
