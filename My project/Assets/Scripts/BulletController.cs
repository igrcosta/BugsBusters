using UnityEngine;

public class BulletController : MonoBehaviour
{

    //variáveis da forma como o tiro vai se comportar
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private float lifetime = 3f;

    public bool isFiredByPlayer = true;
    //esse bool vai verificar à quem pertence a bala atirada, Player atirou, true, inimigo atirou, false

    public AffinityColorENUM bulletColor;
    //cor atual do tiro em questão, seja tanto do inimigo quanto do player

    void Start()
    {
        Destroy(gameObject, lifetime);
        //depois do tempo de lifetime, a bala que possui esse script será destruída
    }

    void Update()
    {
        transform.Translate(transform.forward * bulletSpeed * Time.deltaTime, Space.World);
        //Utilizamos esse transform.Translate por causa da caixa "Is Kinematic" que marcamos
        //no rigidbody da bala, estamos marcando que vamos fazer a movimentação desse gameObject
        //via script, assim o rigidBody não vai dar conflito na movimentação


    }

    private void OnTriggerEnter(Collider other)
    {
        //string que vai detectar qual foi a tag do objeto que recebeu o hit
        string hitTag = other.tag;

        if ((isFiredByPlayer && hitTag == "Player") || (!isFiredByPlayer && hitTag == "Enemy")) 
        {
            //se o mesmo gameobject que atirou, foi o mesmo que recebeu hit
            return;
            //ignora o resto do código e a bala continua (se for um Trigger)
        }

        //lógica de dano abaixo (colocar esquema de cores depois)

        if (!isFiredByPlayer && hitTag == "Player")
        {
            Debug.Log("Player tomou dano de bala " + bulletColor);
        }

        else if (isFiredByPlayer && hitTag == "Enemy")
        {
            //lógica de cores deve ser aplicada aqui
            Debug.Log("ENEMY tomou dano de bala " + bulletColor);
        }

        //se encostou, bala destruída
        Destroy(gameObject);
    }
}
