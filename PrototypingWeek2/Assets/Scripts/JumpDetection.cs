using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpDetection : MonoBehaviour
{
    public Player motionScript;

    private void OnTriggerStay(Collider other)
    {
        Debug.Log("yes");
        if(other.gameObject.layer == LayerMask.NameToLayer("Grounds"))
        {
            Debug.Log("yes2");
            motionScript.canJump = true;
        }
    }
}
