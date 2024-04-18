using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoverTrigger : MonoBehaviour
{
    public bool isBehindCover;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 8)
        {
            isBehindCover = true;
            Debug.Log("Is Behind Cover");
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        isBehindCover = false;
        Debug.Log("Not Behind Cover");
    }
}
