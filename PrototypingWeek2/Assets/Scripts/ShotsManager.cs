using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using System.Linq;
using UnityEngine;
using System;

public class ShotsManager : SingletonBase<ShotsManager>
{
    public Transform effectsParent;
    public GameObject impactEffectPrefab;
    public GameObject lineEffectPrefab;
    List<Shot> allShots = new List<Shot>();
    List<int> readyShotsIndex = new List<int>();
    List<RaycastHit> hitsThisFrame = new List<RaycastHit>();
    public int impactEffectCount;
    public int lineEffectCount;
    Queue<ImpactEffect> impactEffects = new Queue<ImpactEffect>();
    public Queue<LineEffect> lineEffects = new Queue<LineEffect>();
    public Dictionary<Shot, LineEffect> activeShotLines = new Dictionary<Shot, LineEffect>();
    public TMPro.TMP_Text shotCountText;
    public TMPro.TMP_Text rayCountText;

    void Start()
    {
        for (int i = 0; i < impactEffectCount; i++)
            // Pre-load impact effects
            impactEffects.Enqueue(Instantiate(impactEffectPrefab, effectsParent).GetComponent<ImpactEffect>());
        Console.WriteLine(impactEffectCount + " shot impact effects pre-loaded.");
        for (int i = 0; i < lineEffectCount; i++)
            // Pre-load line effects
            lineEffects.Enqueue(Instantiate(lineEffectPrefab, effectsParent).GetComponent<LineEffect>());
        Console.WriteLine(lineEffectPrefab + " line shot effects pre-loaded.");
    }

    JobHandle shotsHandle;
    void FixedUpdate()
    {
        for (int i = 0; i < allShots.Count; i++)
        {
            Shot shot = allShots[i];
            if (shot.completed)
            {
                if (activeShotLines.ContainsKey(shot)) DetachLine(shot);
                allShots.RemoveAt(i);
            }
            else if (shot.readyToFire)
            {
                readyShotsIndex.Add(i);
            }
            else
            {
                shot.Tick();
            }
        }

        shotCountText.text = "Current Shots: " + allShots.Count;
        rayCountText.text = "Updating Rays: " + readyShotsIndex.Count;

        if (readyShotsIndex.Count != 0 && !shotsHandle.Equals(null))
        {
            if (shotsHandle.IsCompleted && !shotRays.IsCreated && !shotResults.IsCreated)
            {
                DoReadyShots(readyShotsIndex.Count, ref hitsThisFrame, ref shotsHandle);
            }
            UpdateAllShots(readyShotsIndex.Count, hitsThisFrame);
            
            readyShotsIndex.Clear();
            hitsThisFrame.Clear();
        }
    }

    NativeArray<RaycastCommand> shotRays;
    NativeArray<RaycastHit> shotResults;
    void DoReadyShots(int count, ref List<RaycastHit> hits, ref JobHandle handle)
    {
        try
        {
            // Create arrays for shot rays and results with variable size
            shotRays = new NativeArray<RaycastCommand>(count, Allocator.TempJob);
            shotResults = new NativeArray<RaycastHit>(count, Allocator.TempJob);
            
            // Get all ready shot rays
            for (int i = 0; i < count; i++)
                shotRays[i] = allShots[readyShotsIndex[i]].ray;
            
            // Schedule raycast job
            handle = RaycastCommand.ScheduleBatch(shotRays, shotResults, count, default(JobHandle));
            
            // Confirm completion of job
            handle.Complete();

            // Immediately send results to a proper list
            hits = shotResults.ToList();

            // Dispose of temporary arrays
            shotRays.Dispose();
            shotResults.Dispose();
        }
        catch (InvalidOperationException e)
        {
            Debug.LogWarning("[CaughtError] Error in DoReadyShots: " + e.ToString());
        }
    }

    void UpdateAllShots(int count, List<RaycastHit> hits)
    {
        for (int i = 0; i < count; i++)
        {
            int r = readyShotsIndex[i];
            Shot shot = allShots[r];
            RaycastHit hit = hits[i];
            Collider collider = hits[i].collider;

            if (collider != null)
            {
                if (collider.tag == "Enemy")
                {
                    Vector3 inDir = -(shot.lastHit.point - hit.point).normalized;

                    if (collider.tag == "Enemy" && !shot.isFirst) 
                    {
                        Health h = collider.GetComponent<Health>();
                        if (h.alive) 
                            h.Damage(shot.damage + .25f * (shot.allHits.Count - 1));
                        else 
                            collider.GetComponent<Rigidbody>().AddForceAtPosition(inDir * shot.damage, hit.point, ForceMode.Impulse);
                    }
                }
                // register the hit in the shot
                shot.Hit(hit);
                // activates an impact effect and cycles it in its queue
                impactEffects.Enqueue(impactEffects.Dequeue().Activate(hits[i].point, hits[i].normal));
            }
            else
            {
                shot.completed = true;
            }

            if (activeShotLines.ContainsKey(shot))
            {
                Console.WriteLine("Updated shot line");
                activeShotLines[shot].Activate(shot.getAllHitPoints().ToArray(), shot.completed);
                RequeueLine(activeShotLines[shot]);
            }
            else
            {
                Console.WriteLine("Used new shot line");
                // fetches a new line from the list, 
                LineEffect newLine = lineEffects.Dequeue();
                activeShotLines.Add(shot, newLine);
                newLine.Activate(shot.getAllHitPoints().ToArray(), shot.completed);
                lineEffects.Enqueue(newLine);
            }
        }
    }

    public void RequeueLine(LineEffect line)
    {
        lineEffects.ToList().Remove(line);
        lineEffects.Enqueue(line);
    }

    public void DetachLine(Shot shot)
    {
        RequeueLine(activeShotLines[shot]);
        activeShotLines.Remove(shot);
    }

    public void RecieveShots(List<Shot> newShots)
    {
        print("Recieved " + newShots.Count + " shots");
        allShots.AddRange(newShots);
    }

    public override void OnDestroy()
    {
        Close();
    }
    public override void OnApplicationQuit()
    {
        Close();
    }
    protected void Close()
    {
        if (shotRays.IsCreated)
            shotRays.Dispose();
        if (shotResults.IsCreated)
            shotResults.Dispose();
        SetDestroying();
    }

    public static Vector3[] TessLine(Vector3[] points, int tessLevel)
    {
        // return original points if array contains less than 2 points (cannot tessellate)
        if (points.Length < 2) return points;
        List<Vector3> tessedLine = new List<Vector3>(points);
        for (int t = 0; t < tessLevel; t++)
        {
            int pointCount = tessedLine.Count;
            for (int i = 0; i < pointCount - 1; i++)
                tessedLine.Insert((i*2)+1, Vector3.Lerp(points[i], points[i+1], .5f));
            points = tessedLine.ToArray();
        }
        return points;
    }
}
