using UnityEngine;

public class BulletController : MonoBehaviour
{
    // variáveis da forma como o tiro vai se comportar
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private float lifetime = 7f;

    public BulletColor bulletColor;
    public bool isFiredByPlayer = true;

    public int PLayerDamage = 10;
    public int EnemyDamage  = 5;

    void Start()
    {
        // depois do tempo de lifetime, a bala que possui esse script será destruída
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Movimentação da bala
        transform.Translate(transform.forward * bulletSpeed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        string hitTag = other.tag;
        int damageToApply = isFiredByPlayer ? PLayerDamage : EnemyDamage;

        // --- REGRAS DE IGNORAR ---
        if ((isFiredByPlayer && hitTag == "Player") || (!isFiredByPlayer && hitTag == "Enemy")) 
        {
            return; 
        }

        // --- LÓGICA DE DANO IKARUGA (Agora com ColorHandler em ColorData ajudando)
        ColorHandler targetHandler = other.GetComponent<ColorHandler>();
        //pegamos o componente que vai tomar o tiro e buscamos o componente que lida com seus cores

        //se ele encontrou algm que tenha o handler de cor...
        if (targetHandler != null)
        {
            targetHandler.HandleHit(damageToApply, bulletColor);
            //chamamos função universal de dano que o handler trata

            Destroy(gameObject);
            //destrói a bala
            return;
            //já que n é um método void, retorna nada
        }
        // Se encostou em qualquer outra coisa (parede, etc.), destrói a bala
        Destroy(gameObject);
    }
}