using UnityEngine;

public class BulletController : MonoBehaviour
{

    //variáveis da forma como o tiro vai se comportar
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private float lifetime = 3f;

    public int bulletColor;
    public bool isFiredByPlayer = true;
    //esse bool vai verificar à quem pertence a bala atirada, Player atirou, true, inimigo atirou, false

    public int damageAmout = 10;

    void Start()
    {
        Destroy(gameObject, lifetime);
        //depois do tempo de lifetime, a bala que possui esse script será destruída

        bulletColor = GameControllerScript.controller.ColorLogic[0];
        //cor atual do tiro em questão, seja tanto do inimigo quanto do player
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
        string hitTag = other.tag;

        if (isFiredByPlayer && hitTag == "Enemy")
        {
            Enemy1 Enemy = other.GetComponent<Enemy1>();

            //se o inimgo tomou tiro e SUA COR é a mesma da bala
            if (Enemy != null && Enemy.currentColor == bulletColor)
            {
                //fazer bosta nenhuma
                Debug.Log("Dano anulado, Enemy mesma cor da bala");
            }
            else if (Enemy != null && Enemy.currentColor != bulletColor)
            {
                //diminuir vida do enemy
                Debug.Log("TOMEI DANO, Enemy cor diferente da bala");
            }
        }

        if ((isFiredByPlayer && hitTag == "Player") || (!isFiredByPlayer && hitTag == "Enemy")) 
        {
            //se o mesmo gameobject que atirou, foi o mesmo que recebeu hit
            return;
            //ignora o resto do código e a bala continua (se for um Trigger)
        }

        //lógica de dano abaixo (colocar esquema de cores depois)

        if (!isFiredByPlayer && hitTag == "Player")
        {
            Player player = other.GetComponent<Player>();
            if (player != null && player.currentColor == bulletColor)
            {
                //fazer bosta nenhuma
                Debug.Log("Dano anulado, Player mesma cor da bala");
            }
            else if (player != null && player.currentColor != bulletColor)
            {
                //causar dano player
                Debug.Log("TOMEI DANO, Player cor diferente da bala");
            }
        }

        //se encostou, bala destruída
        Destroy(gameObject);
    }
    
}
