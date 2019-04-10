using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect : MonoBehaviour
{
    public float duration;
    [HideInInspector]
    public float activateTime;
    [HideInInspector]
    public float lifePercent;

    public virtual void Activate()
    {
        activateTime = Time.time;
        // return this;
    }

    public void Update()
    {
        Tick();
    }

    public void Tick()
    {
        lifePercent = (Time.time - activateTime) / duration;
    }
}
