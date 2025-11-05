using UnityEngine;

public class SafeZoneScript : MonoBehaviour
{
    private bool IsBigger = true;

    private Vector3 BiggerScale = new Vector3(150f,150f,150f);

    private Vector3 SmallerScale = new Vector3(50f,50f,50f);

    private Vector3 ActualScale = new Vector3(0f,0f,0f);

    [SerializeField] private float ShrinkSpeed = 1f;

    //tive que colocar no Awake ao invés do Start, para o sistema de waves já ter referência de forma antecipada
    void Awake()
    {
        GameControllerScript.controller.SafeZone = this;
        //SafeZone se insere dentro do GameController (referência encontrada)
    }

    void Update()
    {
        if (IsBigger)
        {
            ShrinkingScale();
        }
        else
        {
            ResetScale();
        }
    }

    void ResetScale()
    {
        if(IsBigger == false)
        {
            transform.localScale = BiggerScale;
        }
    }

    void ShrinkingScale()
    {
        if(IsBigger == true && transform.localScale.x > SmallerScale.x)
        {

            transform.localScale = Vector3.MoveTowards(
                transform.localScale,
                SmallerScale,
                ShrinkSpeed * Time.deltaTime
            );
        }
        else
        {
            transform.localScale = SmallerScale;
            IsBigger = false;
        }
    }

    public void ResetSize()
    {
        IsBigger = false;
    }

    public void BeginShrinking()
    {
        IsBigger = true;
    }


}
