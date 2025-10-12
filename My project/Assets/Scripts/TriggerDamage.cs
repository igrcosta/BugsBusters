using UnityEngine;

/*public class TriggerDamage : MonoBehaviour
{     //essa classe aplica dano a qualquer objeto que entre em seu trigger e implemente a interface IDamageable
    [SerializeField]
    [Min(0)]
    private int damage = 10;
    private void OnTriggerEnter(Collider collision)
    {
        //verificamos se o objeto que colidiu com o trigger implementa a interface IDamageable
        IDamageable damageble = collision.GetComponent<IDamageable>();
        if (damageble != null)
        {
            damageble.TakeDamage(damage);
        }
    }
}
*/