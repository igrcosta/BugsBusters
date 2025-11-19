using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    public string nextLevel;
    void StartLevel()
    {
        SceneManager.LoadScene("Level01");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartLevel();
        }
    }
}
