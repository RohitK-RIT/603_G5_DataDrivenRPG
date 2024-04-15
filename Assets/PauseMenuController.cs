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
        SceneManager.LoadScene("LevelPrototype");
    }

    public void ShowOptions()
    {
        
    }

    public void QuitGame()
    {
        SceneManager.LoadScene("MainMenu");
    }

    

    //when the Pause key is pressed
    public void OnPause()
    {
        TogglePause();
    }

   
}
