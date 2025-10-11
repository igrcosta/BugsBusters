using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour
{

    [SerializeField] GameObject EnemyPrefab;
    //prefab que vai pegar o inimigo para instaciar

    [SerializeField] int Enemycounter;
    //só um contador básico pro spawner ter um limite de vezes que vai spawnar o inimigo

    [SerializeField] float SpawnCoolDown;
    //variável que vai servir para marcar o intervalo de um spawn para outro

    Coroutine SpawningCycleVar;
    //variável para armazenar no cache


    void Start()
    {
        SpawningCycleVar = StartCoroutine(SpawningCycle());
    }

    //esse item tem que instanciar de tempos em tempos o gameObject do inimigo
    //depois de alguns spawns, fazer ele parar

    //para isso, vamos criar uma CoRoutine que vai fazer spawn de inimigos
    //de tempos em tempos

    //CoRoutines são basicamente semelhantes ao update, mas com a diferença que podemos
    //ter pausas no meio desse processo, o que queremos pro nosso spawner

    IEnumerator SpawningCycle()
    {
        int i = 0;

        while(i < Enemycounter)
        {
            i++;
            Instantiate(EnemyPrefab, transform.position, transform.rotation);
            //instancia um prefab do inimigo na posição do nosso spawner
            
            yield return new WaitForSeconds(SpawnCoolDown);
            //espera alguns segundos definidos pelo SpawnCoolDown
        }
        //assim que chegar no final, a Coroutine vai parar, mas recomendam colocar um:
        //yield break;
    }
}
