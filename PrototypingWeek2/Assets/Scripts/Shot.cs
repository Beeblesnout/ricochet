using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shot
{
    // Variables    
    List<RaycastHit> p_allHits;
    RaycastCommand p_ray;
    public List<RaycastCommand> allRays;
    float p_damage;
    bool p_readyToFire;
    Vector3 p_origin;
    float remainingDistance;
    float hitTime;
    public readonly GameObject owner;
    public readonly int maxHits;
    public readonly float maxDistance;
    public readonly LayerMask layers;
    public bool isFirst = true;
    public bool completed = false;

    // Properties
    public List<RaycastHit> allHits { get => p_allHits; }
    // gets the last hit in the allHits list (returns an empty RaycastHit if list is empty)
    public RaycastHit lastHit { get => p_allHits.Count >= 1 ? p_allHits[p_allHits.Count - 1] : default(RaycastHit); }
    // gets the hit at [end_index - i] in the all hits list (returns an empty RaycastHit if list doesn't contain enough entries)
    public RaycastHit getNthLastHit(int i) { return p_allHits.Count >= i ? p_allHits[p_allHits.Count-i] : default(RaycastHit); }
    public RaycastCommand ray { get => p_ray; }
    public float damage { get => p_damage; }
    public bool readyToFire { get => p_readyToFire; }
    public Vector3 origin { get => p_origin; }

    // Constructor Method
    public Shot(GameObject owner, Transform barrelEnd, Vector3 direction, float baseDamage, int maxHits, float maxDistance, LayerMask mask)
    {
        this.owner = owner;
        p_origin = barrelEnd.position;
        p_allHits = new List<RaycastHit>();
        RaycastHit hit = new RaycastHit();
        hit.point = barrelEnd.position;
        hit.normal = barrelEnd.forward;
        p_allHits.Add(hit);
        this.maxDistance = maxDistance;
        remainingDistance = maxDistance;
        layers = mask;
        p_ray = new RaycastCommand(p_origin, direction, remainingDistance, layers);
        allRays = new List<RaycastCommand>();
        allRays.Add(p_ray);
        p_damage = baseDamage;
        this.maxHits = maxHits;
        hitTime = Time.time + .065f;
        p_readyToFire = true;
    }

    // Variable Update Method
    public void Hit(RaycastHit hit)
    {
        // calculate the hit direction
        Vector3 inDir = -(lastHit.point - hit.point).normalized;
        // calculate the reflected direction
        Vector3 outDir = Vector3.Reflect(inDir, hit.normal);
        // reduce the remaining distance by the distance this hit was
        remainingDistance -= hit.distance;
        // setup the new ray at the updated position, aiming in the out direction, going for the remaining distance, checking on the preset layers 
        p_ray = new RaycastCommand(hit.point, outDir, remainingDistance, layers);
        allRays.Add(p_ray);
        // record the time of this hit
        hitTime = Time.time;
        // unflag ready to fire (activates delay)
        p_readyToFire = false;
        // add new hit position to list of positions
        p_allHits.Add(hit);
        // set isFirst to false from true after the first hit
        isFirst = false;
        if (p_allHits.Count >= maxHits + 1) completed = true;
    }

    // Custom Update Method
    public void Tick()
    {
        if (!completed) p_readyToFire = Time.time - hitTime >= .065f + (.005f * p_allHits.Count);
    }

    /// <summary>
    /// Gets a list of all the recorded hit positions in WS. (+ a final pos)
    /// </summary>
    /// <returns>A Vector3 List of hit positions</returns>
    public List<Vector3> getAllHitPoints()
    {
        List<Vector3> points = new List<Vector3>();
        foreach (RaycastHit hit in p_allHits)
            points.Add(hit.point);
        points.Add(FinalPos());
        return points;
    }

    public Vector3 FinalPos()
    {
        return lastHit.point + p_ray.direction * 3;
    }
}
