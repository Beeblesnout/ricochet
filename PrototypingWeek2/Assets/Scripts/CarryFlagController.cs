using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CarryFlagController : MonoBehaviour
{
    [SerializeField]
    public GameObject carryer;

    void FixedUpdate()
    {
        if (carryer != null)
        {
            transform.position = Vector3.up * 2 + carryer.transform.position;
        }
        transform.LookAt(Camera.main.transform);
    }
}
