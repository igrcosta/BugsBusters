using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //int level = SceneManager.GetActiveScene().buildIndex; //level era next
            //if (level == 2) //se estiver na ultima fase volta pra primeira
            //{
            //    SceneManager.LoadScene(level - 2);
            //}
            //else
                SceneManager.LoadScene(Quarto); //criar cena quarto
        }
    }
}
