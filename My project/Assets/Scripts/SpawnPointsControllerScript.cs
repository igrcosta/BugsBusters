using UnityEngine;

public class SpawnPointsControllerScripts : MonoBehaviour
{
    [SerializeField] GameObject[] Spawners;
    //criamos um array com os spawners, ele vai ser responsável por aleatorizar quais spawners vão estar ativos

    private GameObject SpawnerSelected;

    void Awake()
    {
        for(int u = 0; u < Spawners.Length-1;u++)
        {
            GameObject actualSpawner = Spawners[u];
            actualSpawner.SetActive(false);
        }

        Activation();
    }

    private void Activation()
    {
        for(int i = 0; i < 2; i++)
        {
            int SpawnerSelected = Random.Range(0,(Spawners.Length));
            if(!Spawners[SpawnerSelected].activeInHierarchy)
            {
                Spawners[SpawnerSelected].gameObject.SetActive(true);
                //se o spawner aleatório selecionado não estiver ativo na Hierarquia,
                //ative ele, se não, só pula
            }
            else
            {
                
            }
        }
    }
}
