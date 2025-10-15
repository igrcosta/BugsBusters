using UnityEngine;

public class SafeZoneScript : MonoBehaviour
{
    private bool IsBigger = true;

    private Vector3 BiggerScale = new Vector3(100f,100f,100f);

    private Vector3 SmallerScale = new Vector3(10f,10f,10f);

    private Vector3 ActualScale = new Vector3(0f,0f,0f);

    [SerializeField] private float ShrinkSpeed = 1f;

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

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("O Player tá colidindo com a safezone!");
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
