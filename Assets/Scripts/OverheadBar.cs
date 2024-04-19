using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverheadBar : MonoBehaviour
{

    public HealthBarController healthBarController;
    public Unit playerUnit;
    // Start is called before the first frame update
    private void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        healthBarController.maxHealthBar = playerUnit.maxHP;
        healthBarController.healthBar = playerUnit.currentHP;
        healthBarController.healthProgressUI.fillAmount = healthBarController.healthBar / healthBarController.maxHealthBar;
    }
}
