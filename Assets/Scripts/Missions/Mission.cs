using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Mission : ScriptableObject
{
    public string scene_name;
    public string mission_title;
    [SerializeField, TextArea(10,10)]
    public string mission_description;
    public float difficulty_indicator; 
}
