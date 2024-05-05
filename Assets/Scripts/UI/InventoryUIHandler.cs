using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InventoryUIHandler : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject invContainer; // scrollable panel which contains every item the player has collected
    public GameObject invItemPrefab;
    public List<GameObject> weaponSlots;
    


    //when this object is shown on the UI, correctly show each weapon and accessory a player has equipped 
    public void OnEnable()
    {
        UpdateDisplay();
    }

    public void ShowWeapEquipChoices(string unitName)
    {
        Dictionary<Weapon, string> invWeapons = InventoryManager.Instance.inventory;
        int yOffset = 0;
        invContainer.SetActive(true);
               

        foreach (Weapon weapon in invWeapons.Keys)
        {
            GameObject newButton = GameObject.Instantiate(invItemPrefab, invContainer.transform, true);
            newButton.GetComponentInChildren<TextMeshProUGUI>().text = weapon.weapon_name;
            Vector3 buttonPosition = invContainer.transform.position + new Vector3((invContainer.GetComponent<RectTransform>().rect.width / 2), -55, 0);
            buttonPosition += new Vector3(0, -100 * yOffset, 0);
            newButton.transform.position = buttonPosition;

            newButton.GetComponent<Button>().onClick.AddListener(() => EquipWeapon(weapon, unitName));

            yOffset++;
        }
    }

    public void EquipWeapon(Weapon weapon, string unitName)
    {
        invContainer.SetActive(false);

        //update name of slot to match new equipped weapon
        //slot_text.text = weapon.weapon_name;

        Unit unitObj = GameObject.Find(unitName).GetComponent<Unit>();

        InventoryManager.Instance.SetEquippedWeapon(weapon, unitObj);
        InventoryManager.Instance.SaveInventoryData();

        //clear the panel of all buttons
        foreach (Transform button in invContainer.transform)
        {
            Destroy(button.gameObject);
        }

        UpdateDisplay();

    }

    public void EquipAccesory()
    {

    }

    private void UpdateDisplay()
    {
        //get every unit, get their equipped items, and display it on the UI
        Dictionary<Weapon, string> invWeapons = InventoryManager.Instance.inventory;

        GameObject[] weaponSlots = GameObject.FindGameObjectsWithTag("WeaponSlot");

        int index = 0;
        foreach (Weapon weapon in invWeapons.Keys)
        {
            weaponSlots[index].GetComponentInChildren<TextMeshProUGUI>().text = weapon.weapon_name;
            index++;
        }

    }
}
