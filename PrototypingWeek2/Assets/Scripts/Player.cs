﻿using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterMotion))]
public class Player : MonoBehaviour
{
    public PlayerUser user;
    CharacterMotion motor;
    Health health;
    GunController gun;

    //Bool determining if player can jump with associated get/set method
    private bool canJump = true;
    public bool CanJump
    {
        get { return canJump; }
        set { canJump = value; }
    }

    bool disableInput;

    void Awake()
    {
        motor = GetComponent<CharacterMotion>();
        health = GetComponent<Health>();
        gun = GetComponentInChildren<GunController>();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // if (!user.IsMine) return;
        motor.prevLookInput = motor.lookInput;
        if (!disableInput)
        {
            // Movement Input
            motor.moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

            // Mouse Input
            motor.lookInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

            // Jump Input
            if(Input.GetButtonDown("Jump") && canJump == true)
            {
                motor.queueJump = true;
                canJump = false;
            } 

            // Gun Scrolling
            if (Input.mouseScrollDelta.y > 0)
                motor.gun.ScrollGun(+1);
            else if (Input.mouseScrollDelta.y < 0)
                motor.gun.ScrollGun(-1);

            gun.shooting = Input.GetButton("Fire1");
        }
        else
        {
            motor.moveInput = Vector2.zero;
            motor.lookInput = Vector2.zero;
        }

        if (!health.alive)
        {
            
        }
    }

    public void UpdateGunText()
    {
        UIManager.Instance.currentGunText.text = "Gun: " + gun.gun.name;
    }

    public void ToggleInput()
    {
        disableInput = !disableInput;
    }
}
