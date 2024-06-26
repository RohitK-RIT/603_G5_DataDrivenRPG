using System.Collections;
using System.Collections.Generic;
using Core.Managers;
using Core.Managers.Analytics;
using Core.Managers.Events;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    //Initial Created by Tessla. Modified for this project.
    private static GameManager instance;
    [HideInInspector] public bool isPaused = false;
    
    public static GameManager Instance
    {
        get 
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameManager>();

                if (instance == null)
                {
                    GameObject singletonObject = new GameObject("Game Manager");
                    instance = singletonObject.AddComponent<GameManager>();
                }
            }
            return instance;
        }
    }

    void Start()
    {
        // Set up game manager.
        if (instance == null)
        {
            // Makes gamemanager persistent.
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Prevents duplicate instances of the game manager.
            Destroy(gameObject);
        }

        EventManager.Activate();
        AnalyticsManager.Activate();
    }

    void Update()
    {
        // Pause / Unpause game when escape is pressed.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Pause();
        }

        // Restarts game back to main menu
        if (Input.GetKeyDown(KeyCode.P))
        {
            Restart();
        }
    }

    public void Pause()
    {
        // Don't do in start menu
        if (GameUI.Instance.IsScreenActive("Start Screen")) return;

        isPaused =!isPaused;

        // Pause logic
        if (isPaused)
        {
            // Set Pause Screen Active
            GameUI.Instance.SetIsScreenActive("Pause Screen", true);
            Time.timeScale = 0f;
        }
        // Unpause logic
        else if (!isPaused)
        {
            // Set all Screen Inactive (Settings, Options, Pause, etc.)
            GameUI.Instance.SetIsScreenActive("Pause Screen", false);
            GameUI.Instance.SetAllScreensActive(false);
            Time.timeScale = 1f;
        }
    }

    public void Restart()
    {
        // Don't do in start menu
        if (GameUI.Instance.IsScreenActive("Start Screen")) return;

        if (!isPaused)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void Quit()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}

public sealed class StartSessionEvent : GameEvent
{
}
public sealed class EndSessionEvent : GameEvent
{
}

