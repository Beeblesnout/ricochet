using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterMotion))]
public class Player : MonoBehaviour
{
    CharacterMotion motor;
    GunController gun;

    bool disableInput;

    void Awake()
    {
        motor = GetComponent<CharacterMotion>();
        gun = GetComponentInChildren<GunController>();
        gun.gunText = UIManager.Instance.currentGunText;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        motor.prevLookInput = motor.lookInput;
        if (!disableInput)
        {
            // Movement Input
            motor.moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

            // Mouse Input
            motor.lookInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

            // Jump Input
            motor.queueJump ^= Input.GetButtonDown("Jump");

            // Gun Scrolling
            if (Input.mouseScrollDelta.y > 0)
                motor.gun.ScrollGun(+1);
            else if (Input.mouseScrollDelta.y < 0)
                motor.gun.ScrollGun(-1);

            gun.queueShoot ^= Input.GetButton("Fire1") && gun.canShoot;
        }
        else
        {
            motor.moveInput = Vector2.zero;
            motor.lookInput = Vector2.zero;
        }
    }

    public void ToggleInput()
    {
        disableInput = !disableInput;
    }
}
