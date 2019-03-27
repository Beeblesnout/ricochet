using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    public bool isPlayer;
    public List<Gun> guns;
    public int index;
    public Gun gun;

    [SerializeField]
    float shotTime;
    public bool canShoot;
    public bool shooting;
    public CharacterMotion motion;
    public Transform gunEnd;
    public LayerMask mask;

    public GameObject grenade;
    public AudioSource shootSound;
    public ParticleSystem muzzleFlash;
    public ParticleSystem smokePuff;

    public MeshFilter mesh;

    void Awake()
    {
        mesh = GetComponent<MeshFilter>();
        ChangeGun(0);
    }

    private void FixedUpdate()
    {
        if (shooting && canShoot)
        {
            Shoot();
            canShoot = false;
        }

        if (!canShoot && Time.time - shotTime > gun.fireRate)
        {
            canShoot = true;
        }
    }

    public void Shoot()
    {
        if(gun.isProjectile)
        {
            List<Shot> shots = new List<Shot>();
            for (int i = 0; i < gun.shotCount; i++)
                shots.Add(new Shot(motion.gameObject, gunEnd.position, gun.CalcSpreadRot(gunEnd), gun.damage, gun.ricochets, gun.shotRange, mask));
            ShotsManager.Instance.RecieveShots(shots);

            shootSound.Stop();
            shootSound.PlayOneShot(gun.shotSound);
            muzzleFlash.Emit(gun.particleEmitCount);
            smokePuff.Emit(gun.particleEmitCount);
            shotTime = Time.time;
        }
        else
        {
            Debug.Log("I am using projectile");
            for (int i = 0; i < gun.shotCount; i++)
            {
                GameObject spawnedGrenade = Instantiate(grenade, gunEnd.position, gunEnd.rotation);
                Grenade spawnedScript = spawnedGrenade.GetComponent<Grenade>();
                spawnedScript.damage = gun.damage;
                spawnedScript.velocity = gun.shotRange;
            }
            shootSound.PlayOneShot(gun.shotSound);
            muzzleFlash.Emit(gun.particleEmitCount);
            smokePuff.Emit(gun.particleEmitCount);
            shotTime = Time.time;
        }
        
    }

    public void ScrollGun(int i)
    {
        index = (int)Mathf.Repeat(index + i, guns.Count);
        ChangeGun(index);
    }

    public void SetGun(int i)
    {
        if (i < 0 || i >= guns.Count) return;
        index = i;
        ChangeGun(index);
    }

    public int GetGunID()
    {
        return guns.IndexOf(gun);
    }

    public void ChangeGun(int i)
    {
        gun = guns[i];
        mesh.mesh = gun.mesh;
        GetComponent<Player>()?.UpdateGunText();
    }
}
