using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterMotion))]
public class Player : MonoBehaviour
{
    public PlayerUser user;
    CharacterMotion motor;
    Health health;
    GunController gun;

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
            motor.queueJump ^= Input.GetButtonDown("Jump");

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
