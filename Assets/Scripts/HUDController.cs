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
    int focusedIndex = 0;
    List<Unit> selectedUnits;

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
        if (selectedUnits.Count > 0 && Input.GetKeyDown(KeyCode.Tab))
        {
            focusedIndex = (focusedIndex + 1) % selectedUnits.Count;
            focusedUnit = selectedUnits[focusedIndex];
            UpdateFocusedUnit();
        }
    }

    void OnUnitsSelected(List<Unit> units)
    {
        if (units.Count > 0)
        {
            selectedUnits = units;
            focusedIndex = 0;
            UpdateFocusedUnit();
        }
        else
        {
            selectedUnits.Clear();

            // disable all ability buttons
            unitNameText.text = "";
            foreach (Button b in abilityButtons)
            {
                b.onClick.RemoveAllListeners();
                b.gameObject.SetActive(false);
            }
        }
    }

    void UpdateFocusedUnit()
    {
        // Switch to listen to the new focused unit's Damage Taken event
        if (focusedUnit)
        {
            focusedUnit.OnDamageTaken -= UpdateUnitNameDisplay;
            focusedUnit.OnHealed -= UpdateUnitNameDisplay;
            focusedUnit.Unfocus();
        }
        focusedUnit = selectedUnits[focusedIndex];
        focusedUnit.OnDamageTaken += UpdateUnitNameDisplay;
        focusedUnit.OnHealed += UpdateUnitNameDisplay;
        focusedUnit.Focus();

        UnitAbility[] abilities = focusedUnit.GetComponents<UnitAbility>();
        
        // Set the Ability Buttons; Only show abilities of friendly units
        int i = 0;
        if (focusedUnit.Hostility == Hostility.Friendly)
        {
            for (; i < abilities.Length; i++)
            {
                if (i < abilityButtons.Count)
                {
                    UnitAbility a = abilities[i];

                    abilityButtons[i].onClick.RemoveAllListeners();
                    abilityButtons[i].onClick.AddListener(() =>
                    {
                        focusedUnit.SetQueuedAbility(a);
                    });
                    // Update displays
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

        // Display the unit name and HP
        UpdateUnitNameDisplay(focusedUnit.currentHP);
    }

    void UpdateUnitNameDisplay(float newHP)
    {
        unitNameText.text = $"{focusedUnit.name} ({(int)newHP}/{(int)focusedUnit.maxHP} HP)";
    }
}
