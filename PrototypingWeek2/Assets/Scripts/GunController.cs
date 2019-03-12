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
        RaycastHit aimHit;
        if (Physics.Raycast(gunEnd.position, gunEnd.forward, out aimHit, gun.shotRange))
        {
            UIManager.Instance.aimCrosshair.position = Camera.main.WorldToScreenPoint(aimHit.point);
            SetRicochetLine(aimHit.point, Vector3.Reflect(gunEnd.forward, aimHit.normal), aimHit.distance, 11);
        }
        else
        {
            UIManager.Instance.aimCrosshair.position = new Vector3(Screen.width / 2, Screen.height / 2);
            // UIManager.Instance.reflectCrosshair.position = Vector3.positiveInfinity;
            SetRicochetLine();
        }
    }

    void SetRicochetLine()
    {
        LineRenderer line = UIManager.Instance.aimCrosshair.GetComponent<LineRenderer>();
        line.positionCount = 0;
    }

    public AnimationCurve ricochetLineDistanceCurve;
    void SetRicochetLine(Vector3 point, Vector3 dir, float dist, int pointCount)
    {
        LineRenderer line = UIManager.Instance.aimCrosshair.GetComponent<LineRenderer>();
        line.positionCount = pointCount;
        line.widthMultiplier = .025f + ricochetLineDistanceCurve.Evaluate(dist / 100);
        Vector3[] pos = new Vector3[pointCount];
        for (int i = 0; i < line.GetPositions(pos); i++)
        {
            line.SetPosition(i, point + dir * i * ricochetLineDistanceCurve.Evaluate(dist / 75) * 3);
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
        gunText.text = "Gun: " + gun.name;
        mesh.mesh = gun.mesh;
    }
}
