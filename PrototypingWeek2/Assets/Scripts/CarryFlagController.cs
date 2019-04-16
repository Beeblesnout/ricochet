using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CarryFlagController : MonoBehaviour
{
    //To store the current flag carrier.
    [SerializeField]
    public GameObject carryer;

    void FixedUpdate()
    {
        //Update flag location while the flag is being carried.
        if (carryer != null)
        {
            transform.position = Vector3.up * 2 + carryer.transform.position;
        }
        transform.LookAt(Camera.main.transform);
    }
}
