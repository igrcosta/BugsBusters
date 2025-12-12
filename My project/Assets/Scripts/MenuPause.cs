using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class MenuPause : MonoBehaviour
{
    GameControllerScript gameControllerScript;
    
    public void MainMenu()
    {
        GameControllerScript.ReturnToMenuAndCleanup(); 
    }

}