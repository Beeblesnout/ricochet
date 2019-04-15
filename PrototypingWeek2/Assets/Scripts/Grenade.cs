using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    private bool canExplode = false;
    private bool hasExploded = false;

    [SerializeField]
    private SphereCollider explosionRadius;
    public GameObject explosionObject;

    [Space]
    [SerializeField]
    private float explodeTimer;
    private float timer;
    public float damage;
    public float velocity;

    void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.AddForce(transform.forward * velocity, ForceMode.Impulse);
        transform.rotation = Random.rotation;
        timer = explodeTimer;
    }

    void Update()
    {
        if(hasExploded)
        {
            Instantiate(explosionObject, transform.position, Quaternion.identity);
            Destroy(this.gameObject);
        }
        if (canExplode)
        {
            timer -= Time.deltaTime;
            if(timer <= 0)
            {
                explosionRadius.enabled = true;
                hasExploded = true;
            }
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.tag == "Environment" && !canExplode)
        {
            canExplode = true;
        }
        if(other.gameObject.tag != "Environment" && canExplode)
        {
            timer = 0;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player" || other.gameObject.tag == "Enemy")
        {
            Debug.Log("Damage");
        }
    }
}
