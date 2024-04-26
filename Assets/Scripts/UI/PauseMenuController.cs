using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class PauseMenuController : MonoBehaviour
{
    public GameObject pauseUI;
    public bool isPaused;

     void Start()
     {
        //game is assumed to start unpaused 
        pauseUI.SetActive(false);
        isPaused = false; 
     }

    public void TogglePause()
    {
        //toggle on
        if (!isPaused)
        {
            pauseUI.SetActive(true);
            isPaused = true;
            Time.timeScale = 0;
        }
        //toggle off
        else if (isPaused)
        {
            pauseUI.SetActive(false);
            isPaused = false; 
            Time.timeScale = 1; 
        }
    }

    public void ResetGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("Level2Prototype");
    }


    public void QuitMission()
    {
        Time.timeScale = 1;
        OpenScene("Overworld");
    }

    public void OpenScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    //when the Pause key is pressed
    public void OnPause()
    {
        TogglePause();
    }

   
}
