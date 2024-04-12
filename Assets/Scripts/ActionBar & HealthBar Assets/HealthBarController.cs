using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarController : MonoBehaviour
{
    [Header("HealthBar Main Params")]
    public float healthBar = 100f;
    public float maxHealthBar = 100f;

    [Header("HealthBar Drain Params")]
    [Range(0, 50)] public float healthDrain;
    //[Range(0, 50)] public float actionRegen;

    [Header("HealthBar UI Params")]
    public Image healthProgressUI = null;
    public CanvasGroup sliderCanvasGroup = null;

    [SerializeField] FieldOfView fieldOfView;
    [SerializeField] ActionBarController actionBarController;
    
    bool isHit;
    bool isDead;

    /*
    private void Start()
    {
        //sliderCanvasGroup.alpha = 1;
        
    }
    private void Update()
    {
        if(healthBar > 0 && isHit == true)
        {
            //sliderCanvasGroup.alpha = 1;
            healthBar -= healthDrain;
            healthProgressUI.fillAmount = healthBar / maxHealthBar;
            isHit = false;
            Debug.Log("Hit");
        }
        else if (healthBar <= 0)
        {
            isDead = true;
            Debug.Log("Dead!");
        }
        else
        {
            Debug.Log("Idle!");
        }
        
        // Can be modifed to be integrated with AJs stuff
        if (Input.GetKeyDown(KeyCode.Space) && fieldOfView.canSeePlayer == true && actionBarController.isUseable == true)
        {
            isHit = true;
        }
    }
    */
}
