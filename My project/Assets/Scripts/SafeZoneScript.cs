using UnityEngine;

public class SafeZoneScript : MonoBehaviour
{
    private bool isShrinking = false;
    private bool isActive = false;

    private Vector3 InitialScale = new Vector3(150f,150f,150f);

    private Vector3 ShrinkStartScale = new Vector3(60f,60f,60f);

    private Vector3 SmallerScale = new Vector3(0f,0f,0f);

    [SerializeField] private float ShrinkSpeed = 1f;

    //tive que colocar no Awake ao invés do Start, para o sistema de waves já ter referência de forma antecipada
    void Awake()
    {
        GameControllerScript.controller.SafeZone = this;
        //SafeZone se insere dentro do GameController (referência encontrada)

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
