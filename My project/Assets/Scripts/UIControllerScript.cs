using UnityEngine;
using UnityEngine.SceneManagement;

public class UIControllerScript : MonoBehaviour
{
    public void Retry()
    {
        SceneManager.LoadScene(1);
    }
}
