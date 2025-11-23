using UnityEngine;

public class TutorialController : MonoBehaviour
{
    public static TutorialController controller;

    [Header("Referências")]

    public TutorialPlayer PlayerTutorialRef;

    [Header("Materiais que todos usam")]
    public Material MatFirst, MatSecond;

    private void Awake()
    {
        Singleton();
        
    }

    private void Singleton()
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
}
