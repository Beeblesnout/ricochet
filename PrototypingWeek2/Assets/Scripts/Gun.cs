using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Gun : ScriptableObject
{
    public string gunName;
    public float damage;
    public float fireRate;
    public int ricochets;
    public int shotCount;
    public float shotRange;
    public float bulletSpread;
    public int particleEmitCount;
    public Mesh mesh;
    public AudioClip shotSound;

    public Vector3 CalcSpreadRot(Transform gunEnd)
    {
        Quaternion angle = Quaternion.AngleAxis(Random.Range(0f, 360f), gunEnd.forward);
        Quaternion spread = Quaternion.AngleAxis(bulletSpread * Random.value, Vector3.Cross(gunEnd.forward, gunEnd.up));
        return angle * spread * gunEnd.forward;
    }
}
