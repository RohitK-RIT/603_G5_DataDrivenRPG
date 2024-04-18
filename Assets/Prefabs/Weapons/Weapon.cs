using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu]
public class Weapon : ScriptableObject
{
    public string weapon_name;  
    public int damage_per_shot;
    public int shots_per_attack;
    public float baseAccuracy;
    public float range;
    public int weight;
}
