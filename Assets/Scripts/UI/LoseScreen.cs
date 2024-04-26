using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoseScreen : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (SelectionManager.allFriendlyUnits.Count == 0)
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
        SceneManager.LoadScene("Overworld");
    }
}
