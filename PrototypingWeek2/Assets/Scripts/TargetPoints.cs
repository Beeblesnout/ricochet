using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetPoints : MonoBehaviour
{
    public float points;
    public UIManager manager;

    public void DirectHit() 
    {
        manager.DirectTargetHit(points);
    }

    public void RicochetHit()
    {
        manager.RicochetTargetHit(points);
    }
}
