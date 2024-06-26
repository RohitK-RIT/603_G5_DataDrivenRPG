using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIRotator : MonoBehaviour
{
    public Transform trans;
    private Vector3 offset = new Vector3(0, 180, 0);
    // Start is called before the first frame update
    void Start()
    {
        trans = GameObject.Find("Camera").GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = trans.rotation; 
        transform.Rotate(offset);
    }
}
