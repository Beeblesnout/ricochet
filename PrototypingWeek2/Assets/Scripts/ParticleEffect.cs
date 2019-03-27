using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleEffect : Effect
{
    // Type of particle emission
    public enum EmissionType
    {
        BURST, DECAY //, REPEATED_BURST
    }

    // Public Vars
    public EmissionType emitType;
    public int emitCount;
    public AnimationCurve decayCurve;
    
    // Components
    ParticleSystem particles;

    void Awake()
    {
        particles = GetComponent<ParticleSystem>();
    }

    public override void Activate()
    {
        activateTime = Time.time;
        if (emitType == EmissionType.BURST) 
        {
            particles.Clear();
            particles.Emit(emitCount);
        }
    }

    void Emit() 
    {
        particles.Emit((int)(emitCount * decayCurve.Evaluate(lifePercent)));
    }
    
    public new void Update()
    {
        if (emitType == EmissionType.DECAY) Emit();
        Tick();
    }
}
