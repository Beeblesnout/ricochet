using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    public bool isPlayer;
    public Gun[] guns;
    public int index;
    public Gun gun;

    [SerializeField]
    float shotTime;
    public bool canShoot;
    public bool queueShoot;
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
        switch(gun.isProjectile)
        {
            case false:
                List<Shot> shots = new List<Shot>();
                for (int i = 0; i < gun.shotCount; i++)
                    shots.Add(new Shot(motion.gameObject, gunEnd, gun.CalcSpreadRot(gunEnd), gun.damage, gun.ricochets, gun.shotRange, mask));
                ShotsManager.Instance.RecieveShots(shots);

                shootSound.Stop();
                shootSound.PlayOneShot(gun.shotSound);
                muzzleFlash.Emit(gun.particleEmitCount);
                smokePuff.Emit(gun.particleEmitCount);
                shotTime = Time.time;
                break;

            case true:
                Debug.Log("I am using projectile");
                for (int i = 0; i < gun.shotCount; i++)
                {
                    GameObject spawnedGrenade = Instantiate(grenade, gunEnd.position, gunEnd.rotation);
                    Grenade spawnedScript = spawnedGrenade.GetComponent<Grenade>();
                    spawnedScript.damage = gun.damage;
                    spawnedScript.velocity = gun.shotRange;
                }
                shotTime = Time.time;
                break;

            default:
                Debug.Log("Invalid gun");
                break;
        }
        
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
        mesh.mesh = gun.mesh;
        GetComponent<Player>()?.UpdateGunText();
    }
}
