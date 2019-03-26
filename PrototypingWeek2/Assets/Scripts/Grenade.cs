﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    private bool canExplode = false;
    private float explodeTimer;
    public float damage;
    public float velocity;

    void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        Debug.Log("Grenade spawned");
        Debug.Log(damage);
        rb.AddForce(transform.forward * velocity, ForceMode.Impulse);
        transform.rotation = Random.rotation;
    }

    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision other)
    {
       if(other.gameObject.tag == "Environment" && !canExplode)
        {
            Debug.Log("OINGO BOINGO BROTHERS");
        }
    }
}
