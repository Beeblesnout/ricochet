using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    public bool isPlayer;
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
            shots.Add(new Shot(gunEnd, gun.CalcSpreadRot(gunEnd), gun.damage, gun.ricochets, gun.shotRange, mask));
        ShotsManager.Instance.RecieveShots(shots);

        shootSound.Stop();
        shootSound.PlayOneShot(gun.shotSound);
        muzzleFlash.Emit(gun.particleEmitCount);
        smokePuff.Emit(gun.particleEmitCount);
        shotTime = Time.time;
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
        if (isPlayer) gunText.text = "Gun: " + gun.name;
        mesh.mesh = gun.mesh;
    }
}
