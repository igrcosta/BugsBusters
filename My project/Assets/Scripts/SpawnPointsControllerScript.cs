using UnityEngine;
using UnityEngine.SceneManagement;

public class SpawnPointsControllerScripts : MonoBehaviour
{
    GameObject[] SpawnPoints;
    //criamos um array com os spawners, ele vai ser responsável por aleatorizar quais spawners vão estar ativos

    private GameObject SpawnerSelected;

    void Start()
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
        for(int i = 0; i < 2; i++)
        {
            int SpawnerSelected = Random.Range(0,(SpawnPoints.Length));
            if(!SpawnPoints[SpawnerSelected].activeInHierarchy)
            {
                SpawnPoints[SpawnerSelected].gameObject.SetActive(true);
                //se o spawner aleatório selecionado não estiver ativo na Hierarquia,
                //ative ele, se não, só pula
            }
            else
            {
                
            }
        }
    }
}
