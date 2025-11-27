using UnityEngine;
using System.Collections;
//System.Collections é pra coroutines

public class Shockwaves : MonoBehaviour
{
    [SerializeField] float GrowingSpeed = 10f;
    [SerializeField] int Damage = 15;
    [SerializeField] float Lifetime = 5f;

    public BulletColor ShockwaveColor;

    private Vector3 InitialScale = new Vector3(0,0,0);
    private Vector3 FinalScale = new Vector3 (2.5f,2.5f,2.5f);
    private Vector3 ActualScale;

    private Vector3 GrowingVector; 

    void Start()
    {
        //no momento do start, ele precisa começar a crescer e flowdase
        //começa em 0, precisa chegar até 2

        transform.localScale = InitialScale;
        ActualScale = InitialScale;

        GrowingVector = new Vector3(GrowingSpeed, GrowingSpeed, GrowingSpeed);
        StartCoroutine("GrowingScale");
    }

    IEnumerator GrowingScale()
    {
        while(ActualScale != FinalScale)
        {
            ActualScale+=GrowingVector;
            transform.localScale = ActualScale;
            yield return null;
        }
        Destroy(gameObject);
        yield break;
    }

    void OnTriggerEnter(Collider other)
    {
        ColorHandler targetHandler = other.GetComponent<ColorHandler>();
        if (targetHandler != null)
        {
            targetHandler.HandleHit(Damage, ShockwaveColor);
        }
    }
}
