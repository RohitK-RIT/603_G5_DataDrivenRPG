using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionBarController : MonoBehaviour
{
    [Header("ActionBar Main Params")]
    public float actionBar = 0f;
    public float maxActionBar = 0.800f;

    [Header("ActionBar Regen Params")]
    //[Range(0, 50)] public float actionDrain;
    [Range(0, 50)] public float actionRegen;

    [Header("ActionBar UI Params")]
    public Image actionProgressUI = null;
    //public CanvasGroup sliderCanvasGroup = null;

    [HideInInspector] public bool isUseable;
    [HideInInspector] public bool isFilling;

    /*
    private void Start()
    {
        //sliderCanvasGroup.alpha = 1;
        
    }

    private void Update()
    {
        if(actionBar < 100 && isUseable == false)
        {
            isFilling = true;
            //sliderCanvasGroup.alpha = 1;
            actionProgressUI.fillAmount = actionBar / maxActionBar;
            actionBar += actionRegen * Time.deltaTime;
            Debug.Log("Filling Action Bar");
        }
        else
        {
            isFilling = false;
            isUseable = true;
            Debug.Log("Action Bar Filled!");
        }

        if (Input.GetKeyDown(KeyCode.Space) && isUseable == true)
        {
            actionBar = 0;
            isUseable = false;
        }
    }
    */
}
