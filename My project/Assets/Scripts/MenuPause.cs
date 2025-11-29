using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using JetBrains.Annotations;


public class MenuPause : MonoBehaviour
{
    public GameObject uiConfig;
    bool visivel = false;

    

    void Start()
    {
       

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            visivel = !visivel;
            uiConfig.SetActive(visivel);

            if (visivel)
            {
                Time.timeScale = 0.0f;
            }
            else
            {
                Time.timeScale = 1.0f;
            }
        }
    }

}