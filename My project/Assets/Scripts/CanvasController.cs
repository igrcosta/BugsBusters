using UnityEngine;
using UnityEngine.SceneManagement;

public class CanvasController : MonoBehaviour
{
    public void BeginGame()
    {
        SceneManager.LoadScene(1);
    }
    public void Retry()
    {
        int lastScene = GameControllerScript.LastLevelSceneIndex;

        GameControllerScript.controller.CleanUpGame();

        SceneManager.LoadScene(lastScene);
    }

    public void MenuGame()
{
    // Limpa o estado e volta para o Menu (Índice 0)
    GameControllerScript.ReturnToMenuAndCleanup(); 
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
}
