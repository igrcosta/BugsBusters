using UnityEngine;
using UnityEngine.SceneManagement;

public class CanvasController : MonoBehaviour
{
    public void Retry()
    {
        SceneManager.LoadScene(1);
    }
}
