using UnityEngine;
using UnityEngine.SceneManagement;

public class SpawnPointsControllerScripts : MonoBehaviour
{
    GameObject[] SpawnPoints;
    //criamos um array com os spawners, ele vai ser responsável por aleatorizar quais spawners vão estar ativos

    private GameObject SpawnerSelected;

    //tive que colocar no Awake ao invés do Start, para o sistema de waves já ter referência de forma antecipada
    void Awake()
    {
        SpawnPoints = new GameObject[transform.childCount];
        for (int i=0;i<transform.childCount;i++)
        {
            SpawnPoints[i] = transform.GetChild(i).gameObject;
        }
        if (GameControllerScript.controller != null)
        {
            GameControllerScript.controller.RegisterSpawnManager(this);
            }
            else
            {
                Debug.LogError("GameController não encontrado na cena!");
                }
    }

    public void ResetSpawners()
    {
        for(int u = 0; u < SpawnPoints.Length;u++)
        {
            GameObject actualSpawner = SpawnPoints[u];
            actualSpawner.SetActive(false);
        }
    }

    public void Activation()
    {
        int currentWaveTotalEnemies = 0;
        //int para contar o total de inimigos que vão spawnar, assim o player só ganha se matar esse número

        for(int i = 0; i < 3; i++)
        {
            int SpawnerSelected = Random.Range(0,(SpawnPoints.Length));
            if(!SpawnPoints[SpawnerSelected].activeInHierarchy)
            {
                GameObject selectedSpawner = SpawnPoints[SpawnerSelected];

                selectedSpawner.gameObject.SetActive(true);
                //se o spawner aleatório selecionado não estiver ativo na Hierarquia,
                //ative ele, se não, só pula

                Spawner SpawnerScript = selectedSpawner.GetComponent<Spawner>();

                currentWaveTotalEnemies += SpawnerScript.Enemycounter;

                SpawnerScript.StartSpawning();
            }
            else
            {
                
            }
        }

        //depois de já ativar todos os spawners...
        GameControllerScript.controller.SetTotalEnemiesToKill(currentWaveTotalEnemies);
    }
}
