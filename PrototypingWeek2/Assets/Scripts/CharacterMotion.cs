using System.Collections;
using System.Collections.Generic;
using Popcron.Networking;
using UnityEngine;

public class CharacterMotion : IDedObject<CharacterMotion>
{
    public int playerTeamID;

    public Transform head;
    public bool disableMovement;
    public float accel;
    public float deccel;
    public float maxSpeed;
    public float clampRate;
    public float lookSensitivity;
    public float jumpStrength;
    
    public GunController gun;
    public float gunRotateRate;
    public Vector3 gunHeadOffset;
    public float gunPosRate;

    public Health health;

    [HideInInspector]
    public Rigidbody rb;
    [HideInInspector]
    public Vector2 moveInput;
    [HideInInspector]
    public Vector2 lookInput;
    [HideInInspector]
    public Vector2 prevLookInput;
    [HideInInspector]
    public bool queueJump;
    [HideInInspector]
    public Vector3 headFlatForward;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        health = GetComponent<Health>();
        health.onDeath.AddListener(KillPop);
    }


    // temporary movement velocity
    Vector3 vel;
    private void FixedUpdate()
    {
        // Look Rotation
        head.Rotate(Vector3.up, lookInput.x * lookSensitivity, Space.World);
        head.Rotate(head.right, -lookInput.y * lookSensitivity, Space.World);

        if (health.alive) {
            // Movement
            headFlatForward = new Vector3(head.forward.x, 0, head.forward.z).normalized;
            vel += (head.right * moveInput.x + headFlatForward * moveInput.y) * accel;

            if (vel.magnitude > maxSpeed)
                vel = Vector3.Lerp(vel, vel.normalized * maxSpeed, clampRate);

            if (moveInput.magnitude == 0)
                vel -= vel.normalized * vel.magnitude * deccel;

            rb.velocity = new Vector3(vel.x, rb.velocity.y, vel.z);

            if (queueJump && Physics.Raycast(transform.position + Vector3.down * .9f, Vector3.down, .2f))
            {
                queueJump = false;
                rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                rb.AddForce(Vector3.up * jumpStrength, ForceMode.Impulse);
            }
        }

        // Gun Over-Rotation
        gun.transform.Rotate(Vector3.up, lookInput.x * lookSensitivity * 2, Space.World);
        gun.transform.Rotate(head.right, -lookInput.y * lookSensitivity * 2, Space.World);

        // Gun Rotation
        RaycastHit lookHit;
        bool hasLookHit = Physics.Raycast(head.position, head.forward, out lookHit, 100);
        if (hasLookHit)
            gun.transform.rotation = Quaternion.Lerp(gun.transform.rotation, Quaternion.LookRotation((lookHit.point - head.position).normalized, Vector3.up), gunRotateRate);
        else
            gun.transform.rotation = Quaternion.Lerp(gun.transform.rotation, Quaternion.LookRotation(head.forward, Vector3.up), gunRotateRate);

        // Gun Position
        // Vector3 newGunPos = Vector3.Lerp(gun.position, head.position + head.TransformPoint(gunHeadPosOffset), gunPositionRate);
        // gun.localPosition = gun.parent.InverseTransformPoint(newGunPos);

        // respawn
        if (transform.position.y < -10) 
            transform.position = Vector3.zero;
    }

    void KillPop()
    {
        rb.freezeRotation = false;
        rb.AddExplosionForce(100, (Vector3.up * -.5f) + new Vector3(Random.Range(-1, 1), 0, Random.Range(-1, 1)), 5, 200, ForceMode.Impulse);
    }
}
