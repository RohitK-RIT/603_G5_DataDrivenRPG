using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;

public class HUDController : MonoBehaviour
{
    public GameObject abilitiesBoard;
    public TextMeshProUGUI errorText;
    public TextMeshProUGUI unitNameText;
    Unit focusedUnit;

    List<Button> abilityButtons;

    // Start is called before the first frame update
    void Start()
    {
        abilityButtons = new(abilitiesBoard.GetComponentsInChildren<Button>(true));
        SelectionManager.OnUnitSelectionChanged += OnUnitsSelected;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnUnitsSelected(List<Unit> units)
    {
        if (units.Count > 0)
        {
            UnitAbility[] abilities = units[0].GetComponents<UnitAbility>();
            int i = 0;

            // Only list abilities of friendly units
            if (units[0].Hostility == Hostility.Friendly)
            {
                for (; i < abilities.Length; i++)
                {
                    if (i < abilityButtons.Count)
                    {
                        UnitAbility a = abilities[i];

                        abilityButtons[i].onClick.RemoveAllListeners();
                        abilityButtons[i].onClick.AddListener(() => { a.Execute();} );
                        TextMeshProUGUI[] texts = abilityButtons[i].GetComponentsInChildren<TextMeshProUGUI>(true);
                        abilityButtons[i].GetComponent<Image>().sprite = a.abilitySprite;
                        texts[0].text = a.abilityName;
                        abilityButtons[i].gameObject.SetActive(true);
                    }
                    else
                    {
                        Debug.LogWarning($"A selected unit has more than {abilityButtons.Count} abilities! Only the first {abilityButtons.Count} abilities on this unit will be usable.");
                        break;
                    }
                }
            }
            
            for (; i < abilityButtons.Count; i++)
                abilityButtons[i].gameObject.SetActive(false);

            // Display the unit name
            unitNameText.text = $"{units[0].name} ({(int)units[0].currentHP}/{(int)units[0].maxHP} HP)";

            // Switch to listen to the new focused unit's Damage Taken event
            if (focusedUnit)
            {
                focusedUnit.OnDamageTaken -= UpdateUnitNameDisplay;
                focusedUnit.OnHealed -= UpdateUnitNameDisplay;
            }
            focusedUnit = units[0];
            focusedUnit.OnDamageTaken += UpdateUnitNameDisplay;
            focusedUnit.OnHealed += UpdateUnitNameDisplay;
        }
        else
        {
            // disable all ability buttons
            unitNameText.text = "";
            foreach (Button b in abilityButtons)
            {
                b.onClick.RemoveAllListeners();
                b.gameObject.SetActive(false);
            }
        }
    }

    void UpdateUnitNameDisplay(float newHP)
    {
        unitNameText.text = $"{focusedUnit.name} ({(int)newHP}/{(int)focusedUnit.maxHP} HP)";
    }
}
