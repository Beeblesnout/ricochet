using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    public Gun[] guns;
    public int index;
    public Gun gun;

    public TMPro.TMP_Text gunText;

    float shotTime;
    public bool canShoot;
    public bool queueShoot;
    public Transform gunEnd;
    public LayerMask mask;
    
    public AudioSource shootSound;
    public ParticleSystem muzzleFlash;
    public ParticleSystem smokePuff;

    public Transform crosshair;
    public MeshFilter mesh;

    void Awake()
    {
        mesh = GetComponent<MeshFilter>();
        ChangeGun(0);
    }

    void Update()
    {
        if (Time.time - shotTime > gun.fireRate)
            canShoot = true;

        SetCrosshairPos();
    }

    private void FixedUpdate()
    {
        if (queueShoot)
        {
            Shoot();
            queueShoot = false;
            canShoot = false;
        }
    }

    public void Shoot()
    {
        List<Shot> shots = new List<Shot>();
        for (int i = 0; i < gun.shotCount; i++)
            shots.Add(new Shot(gunEnd, gun.CalcSpreadRot(gunEnd), gun.ricochets, gun.shotRange, mask));
        ShotsManager.Instance.RecieveShots(shots);

        shootSound.Stop();
        shootSound.PlayOneShot(gun.shotSound);
        muzzleFlash.Emit(gun.particleEmitCount);
        smokePuff.Emit(gun.particleEmitCount);
        shotTime = Time.time;
    }

    void SetCrosshairPos()
    {
        RaycastHit gunAimHit;
        if (Physics.Raycast(gunEnd.position, gunEnd.forward, out gunAimHit, 100))
            crosshair.position = Camera.main.WorldToScreenPoint(gunAimHit.point);
        else
            crosshair.position = new Vector3(Screen.width / 2, Screen.height / 2);
    }

    public void ScrollGun(int i)
    {
        index = (int)Mathf.Repeat(index + i, guns.Length);
        ChangeGun(index);
    }

    public void SetGun(int i)
    {
        if (i < 0 || i >= guns.Length) return;
        index = i;
        ChangeGun(index);
    }

    public void ChangeGun(int i)
    {
        gun = guns[i];
        gunText.text = "Gun: " + gun.name;
        mesh.mesh = gun.mesh;
    }
}
