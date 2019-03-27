using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineEffect : Effect
{
    // Variables
    public AnimationCurve effectOverLifetime;
    public float endLength;
    public int lineRes;
    public LineRenderer line;
    public bool isDissipating;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.positionCount = 3;
        isDissipating = false;
    }

    public void Activate(Vector3[] points, bool finalShot)
    {
        isDissipating = finalShot;
        line.positionCount = points.Length + (int)Mathf.Pow(2, lineRes) - 1;
        line.SetPositions(ShotsManager.TessLine(points, lineRes));
        Activate();
    }
    
    public override void Update()
    {
        if (isDissipating)
        {
            base.Update();
            line.material.SetFloat("_Life", effectOverLifetime.Evaluate(lifePercent));
        }
    }
}
