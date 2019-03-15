using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineEffect : Effect
{
    // Variables
    public AnimationCurve effectOverLifetime;
    public float endLength;
    public int tessLevel;
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
        line.positionCount = points.Length;
        line.SetPositions(points);
        TessLine();
        Activate();
    }

    void TessLine()
    {
        // exit if line has less than 2 points (cannot tessellate)
        if (line.positionCount < 2) return;
        Vector3[] positions = new Vector3[line.positionCount];
        line.GetPositions(positions);
        List<Vector3> tessedLine = new List<Vector3>(positions);
        for (int t = 0; t < tessLevel; t++)
        {
            int pointCount = tessedLine.Count;
            for (int i = 0; i < pointCount - 1; i++)
                tessedLine.Insert((i*2)+1, Vector3.Lerp(positions[i], positions[i+1], .5f));
            positions = tessedLine.ToArray();
        }
        line.positionCount = positions.Length;
        line.SetPositions(positions);
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
