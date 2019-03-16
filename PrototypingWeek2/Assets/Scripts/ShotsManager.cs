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
    Queue<LineEffect> lineEffects = new Queue<LineEffect>();
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
            if (allShots[i].completed)
            {
                allShots.RemoveAt(i);
            }
            else if (allShots[i].readyToFire)
            {
                readyShotsIndex.Add(i);
            }
            else
            {
                allShots[i].Tick();
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
            if (hits[i].collider != null)
            {
                if (hits[i].collider.tag == "Enemy")
                {
                    // hits[i].collider.gameObject
                }
                // register the hit in the shot
                shot.Hit(hits[i]);
                // activates an impact effect and cycles it in its queue
                impactEffects.Enqueue(impactEffects.Dequeue().Activate(hits[i].point, hits[i].normal));
            }
            else
            {
                shot.completed = true;
            }

            // activates a line effect and cycles it in its queue
            lineEffects.Enqueue(lineEffects.Dequeue().Activate(shot.getAllHitPoints().ToArray()));

            for (int s = 0; s < shot.getAllHitPoints().ToArray().Length-1; s++)
            {
                    Debug.DrawLine(shot.getAllHitPoints().ToArray()[s], shot.getAllHitPoints().ToArray()[s+1], Color.green, 5f, false);
            }
        }
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
}
