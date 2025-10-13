using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class SpawnerItem : MonoBehaviour
{
    public GameObject itemPrefab;
    public Transform[] spawnPoints;

    private GameObject[] spawnedItems;

    void Start()
    {
        spawnedItems = new GameObject[spawnPoints.Length];
        SpawnAllItems();


    }

    void SpawnAllItems()
    {
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            spawnedItems[i] = Instantiate(itemPrefab, spawnPoints[i].position, spawnPoints[i].rotation);
            var healingItemScript = spawnedItems[i].GetComponent<HealingItemScript>();
            if (healingItemScript != null)
            {
                healingItemScript.SpawnerConfigure(this);
            }
        }

    }

    public void RespawnItem(float tempo)
    {
        StartCoroutine(EsperarRespawn(tempo));
    }

    IEnumerator EsperarRespawn(float tempo)
    {
        yield return new WaitForSeconds(tempo);
        for (int i = 0; i < spawnedItems.Length; i++)
        {
            if (spawnedItems[i] != null && !spawnedItems[i].activeSelf)
            {
                spawnedItems[i].SetActive(true);
                break;
            }
            
        }
    }
}
