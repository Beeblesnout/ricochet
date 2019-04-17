using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpactEffect : Effect
{
    Effect[] effects;
    [FMODUnity.EventRef]
    public string ricochetSound;
    
    void Awake() {
        effects = GetComponentsInChildren<Effect>();
    }

    public ImpactEffect Activate(Vector3 position, Vector3 forward)
    {
        transform.position = position;
        transform.forward = forward;

        Activate();

        for (int i = 0; i < effects.Length; i++)
        {
            effects[i].Activate();
        }

        FMODUnity.RuntimeManager.PlayOneShot(ricochetSound);

        return this;
    }
}
