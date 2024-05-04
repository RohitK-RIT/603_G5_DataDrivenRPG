using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Unity.VisualScripting;
using System.Linq;

public class InventoryManager : MonoBehaviour
{
    public string inventoryPath = "./save_data/inventory.json";

    public Weapon BreacherDefault, SniperDefault, InfiltratorDefault;

    [HideInInspector]
    public static InventoryManager Instance;

    // each inventory item tracks the weapon and the name of its holder
    Dictionary<Weapon, string> inventory;
    struct InventoryData
    {
        public Weapon[] weapons;
        public string[] owners;
    }

    // Start is called before the first frame update
    void Awake()
    {
        // make sure there's only ever one instance of this script active
        if (Instance) 
        {
            enabled = false;
            return;
        }
        
        Instance = this;
        inventory = new Dictionary<Weapon, string>();

        // load in the saved inventory data
        if (File.Exists(inventoryPath))
        {
            InventoryData data = JsonUtility.FromJson<InventoryData>(File.ReadAllText(inventoryPath));

            for (int i = 0; i < data.weapons.Length; i++)
            {
                inventory[data.weapons[i]] = data.owners[i];
            }
        }
        else // no file found, so set default weapons
        {
            inventory[BreacherDefault] = "Breacher";
            inventory[SniperDefault] = "Sniper";
            inventory[InfiltratorDefault] = "Infiltrator";
        }

        foreach (var item in inventory)
        {
            // if a character owns this weapon, have them equip it
            if (!item.Value.Equals(""))
            {
                foreach (Unit u in SelectionManager.allFriendlyUnits)
                {
                    if (u.unitName.Equals(item.Value))
                    {
                        u.equippedWeapon = item.Key;
                        break;
                    }
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance.SaveInventoryData();
            Instance = null;
        }
    }

    /// <summary>
    /// Adds a weapon to the inventory.
    /// </summary>
    /// <param name="weapon">The weapon to add</param>
    /// <returns>
    /// If adding was successful. Unsuccessful means 
    /// he weapon is already in the inventory.
    /// </returns>
    public bool AddToInventory(Weapon weapon)
    {
        if (!inventory.ContainsKey(weapon))
        {
            inventory[weapon] = "";
            return true;
        }
        return false;
    }

    public Weapon FindWeapon(string weaponName)
    {
        foreach (var i  in inventory)
        {
            if (i.Key.weapon_name.Equals(weaponName))
                return i.Key;
        }
        return null;
    }

    // Returns the weapon in the inventory owned by the specific unit whose name is unitName.
    public Weapon FindEquippedWeapon(string unitName)
    {
        foreach (var i in inventory)
        {
            if (i.Value.Equals(unitName))
                return i.Key;
        }
        return null;
    }

    /// <summary>
    /// Equips the specified weapon. If this weapon wasn't 
    /// in the inventory yet, it is automatically added.
    /// </summary>
    /// <param name="weapon">The weapon to equip</param>
    /// <param name="ownedUnit">The unit to equip it on</param>
    public void SetEquippedWeapon(Weapon weapon, Unit ownedUnit)
    {
        if (!weapon || !ownedUnit || ownedUnit.Hostility != Hostility.Friendly) 
            return;

        AddToInventory(weapon);

        // Set the playuer's old weapon to unequipped
        inventory[ownedUnit.equippedWeapon] = "";

        // Actually equip the new weapon
        ownedUnit.equippedWeapon = weapon;
        inventory[weapon] = ownedUnit.unitName;
    }

    void SaveInventoryData()
    {
        int last = 0;
        for (int i = 0; i < inventoryPath.Length; i++)
        {
            if (inventoryPath[i] == '/' || inventoryPath[i] == '\\')
                last = i;
        }
        if (last != 0)
        {
            string subdir = inventoryPath.Substring(0, last);
            if (!Directory.Exists(subdir))
                Directory.CreateDirectory(subdir);
        }
        InventoryData data = new InventoryData();
        data.weapons = inventory.Keys.ToArray();
        data.owners = inventory.Values.ToArray();
        File.WriteAllText(inventoryPath, JsonUtility.ToJson(data));
    }
}
