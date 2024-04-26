using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class OverworldManager : MonoBehaviour
{
    public GameObject desc_container; 
    public TextMeshProUGUI mission_details;
    public TextMeshProUGUI difficulty;
    public TextMeshProUGUI mission_title;
    private string selectedSceneName; 
    bool missionSelected; 
    // Start is called before the first frame update


    //loads the mission description info onto the 
    public void LoadMissionDesc(Mission mission)
    {
        desc_container.SetActive(true);
        mission_details.text = mission.mission_description;
        mission_title.text = mission.mission_title;
        difficulty.text = $"Difficulty: {mission.difficulty_indicator}";
        selectedSceneName = mission.scene_name;
    }
    public void LoadMission()
    {
        SceneManager.LoadScene(selectedSceneName);
    }
}
