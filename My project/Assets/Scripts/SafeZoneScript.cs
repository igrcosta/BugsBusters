using UnityEngine;

public class SafeZoneScript : MonoBehaviour
{
    private bool isShrinking = false;
    private bool isActive = false;

    private Vector3 InitialScale = new Vector3(150f,150f,150f);
    private Vector3 ShrinkStartScale = new Vector3(60f,60f,60f);
    private Vector3 SmallerScale = new Vector3(0f,0f,0f);

    [SerializeField] private float ShrinkSpeed = 1f;

    void Start() 
{
    // Checa SOMENTE GameControllerScript (Exclusivo da Cena de Jogo)
    if (GameControllerScript.controller != null)
    {
        GameControllerScript.controller.SafeZone = this;
    }
    else
    {
        // Se este erro aparecer, é porque o GameController não está na cena (o que é OK na cena tutorial, 
        // mas este erro SÓ deve ocorrer na cena padrão se o GameController estiver faltando)
        Debug.LogError("SafeZoneScript: GameController não encontrado. A SafeZone não será registrada.");
    }

    transform.localScale = InitialScale;
}

    void Update()
    {
        if (isActive && isShrinking)
        {
            ShrinkingScale();
        }
    }

    void ShrinkingScale()
    {
        if(transform.localScale.x <= SmallerScale.x + 0.01f)
        {
            transform.localScale = SmallerScale;
            isShrinking = false;
            isActive = false;
            return;
        }

        transform.localScale = Vector3.MoveTowards(
            transform.localScale,
            SmallerScale,
            ShrinkSpeed * Time.deltaTime
        );
    }

    public void DisableAndReset()
    {
        isActive = false;
        isShrinking = false;
        transform.localScale = InitialScale;
    }

    public void ActivateAndBeginShrinking()
    {
        isActive = true;
        isShrinking = true;
        transform.localScale = ShrinkStartScale;
    }
}