using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; 

public class WinScreen : MonoBehaviour
{


    // Update is called once per frame
    void Update()
    {
        if (SelectionManager.allOtherUnits.Count == 0)
        {
        
            this.GetComponent<Canvas>().enabled = true;
        }
 
    }

    public void ResetGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("Level2Prototype");
    }


    public void QuitGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenu");
    }

}
