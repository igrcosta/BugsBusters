using UnityEngine;

public class ColorHandler : MonoBehaviour
{
        // Define a cor deste objeto no Inspector (ou via script, no caso do Player)
        [SerializeField] public BulletColor currentColor;

        ///<summary>
        ///Esse método aqui é universal, qualquer corno que tomar bala TEM QUE CHAMAR ESSE MÉTODO
        ///AQUI É O LUGAR DA REGRA DO IKARUGA, ISSO NÃO PDOE SER DESVIADO PARA OUTRO LUGAR
        ///</summary>
        /// <param name="bulletDamage">O dano base da bala.</param>
        /// <param name="incomingBulletColor">A cor da bala que atingiu.</param>

        public void HandleHit(int bulletDamage, BulletColor incomingBulletColor)
        {
            //se a cor de quem tomou a bala é diferente da cor da bala...
            if (currentColor != incomingBulletColor)
            {
                //se o cara que tomou o tiro tem a tag Player...
                if(gameObject.CompareTag("Player"))
                {
                    gameObject.GetComponent<Player>()?.TakingDamage(bulletDamage);
                    //pega o componente player que tomou o tiro e executa a função de tomar dano, passando o valor de dano do tiro por parâmetro
                }

                //se quem tomou o grosso foi um cara com tag inimigo...
                else if (gameObject.CompareTag("Enemy"))
                {
                    //palavras não bastam, não dá pra entender, mas ele vai ver qual dos inimigos que tomou esse oco e vai chamar a
                    //função de dano dano daquele que tomou o tiro

                    if(gameObject.TryGetComponent<Enemy1>(out Enemy1 enemy1))
                    {
                        enemy1.TakingDamage(bulletDamage);
                    }
                    else if(gameObject.TryGetComponent<BettleEnemyScript>(out BettleEnemyScript bettle))
                    {
                        bettle.TakingDamage(bulletDamage);
                    }
                    else if(gameObject.TryGetComponent<SmallEnemy>(out SmallEnemy smallEnemy))
                    {
                        smallEnemy.TakingDamage(bulletDamage);
                    }
                    //depois vou colocar a do boss aqui com um else if
                    else
                    {
                        Debug.LogWarning($"Inimigo atingido ({gameObject.name}) mas sem script de vida reconhecido!");
                    }
                }
            }
            else
            {
                //REGRA IMPORTANTE:
                //se a cor do cara que tomou tiro for A MESMA que a da bala...

                //o tiro se destrói e nada acontece, feijoada

                //haha piadas enfadonhas
            }
        }
}

