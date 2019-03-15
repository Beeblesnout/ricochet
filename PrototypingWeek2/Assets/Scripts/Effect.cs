using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Effect : MonoBehaviour
{
    public float duration;
    [HideInInspector]
    public float activateTime;
    public float lifePercent;
    public bool alive;

    public virtual void Activate()
    {
        activateTime = Time.time;
        alive = true;
        lifePercent = 0;
    }

    public virtual void Update()
    {
        alive = lifePercent < 1;
        if (alive) 
            lifePercent = (Time.time - activateTime) / duration;
    }
}
