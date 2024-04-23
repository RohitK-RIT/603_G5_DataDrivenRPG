using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoverTrigger : MonoBehaviour
{
    public bool isBehindCover;

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == 8)
        {
            isBehindCover = true;
            Debug.Log("Is Behind Cover");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        isBehindCover = false;
        Debug.Log("Not Behind Cover");
    }
}
