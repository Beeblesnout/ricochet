using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    private bool canExplode = false;
    private bool hasExploded = false;

    [SerializeField]
    private SphereCollider explosionRadius;

    [Space]
    [SerializeField]
    private float explodeTimer;
    private float timer;
    public float damage;
    public float velocity;

    void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        Debug.Log("Grenade spawned");
        Debug.Log(damage);
        rb.AddForce(transform.forward * velocity, ForceMode.Impulse);
        transform.rotation = Random.rotation;
        timer = explodeTimer;
    }

    void Update()
    {
        if(hasExploded)
        {
            Destroy(this.gameObject);
        }
        switch(canExplode)
        {
            case true:
                timer -= Time.deltaTime;
                Debug.Log(timer);
                if(timer <= 0)
                {
                    Debug.Log("THIS SHOULD EXPLODE");
                    explosionRadius.enabled = true;
                    hasExploded = true;
                }
                break;

            case false:

                break;

            default:

                break;
        }
    }

    private void OnCollisionEnter(Collision other)
    {
       if(other.gameObject.tag == "Environment" && !canExplode)
        {
            Debug.Log("OINGO BOINGO BROTHERS");
            canExplode = true;
        }
    }

    private void OnTriggerExit  (Collider other)
    {
        if (other.gameObject.tag == "Player" || other.gameObject.tag == "Enemy")
        {
            Debug.Log("Damage");
        }
    }
}
