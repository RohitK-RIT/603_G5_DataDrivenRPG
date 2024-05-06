using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class  UnitReference : MonoBehaviour
{
    public Unit connectedUnit;
    public string unitName; 

    public void Start()
    {
        connectedUnit = GameObject.Find(unitName).GetComponent<Unit>();
    }
}
