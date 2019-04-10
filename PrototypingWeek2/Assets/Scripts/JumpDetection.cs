using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpDetection : MonoBehaviour
{
    [SerializeField] private GameObject parent;
    [SerializeField] private Player motionScript;

    // Start is called before the first frame update
    void Start()
    {
        parent = this.transform.parent.gameObject;
        motionScript = parent.GetComponent<Player>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("Grounds"))
        {
            motionScript.CanJump = true;
        }
    }
}
