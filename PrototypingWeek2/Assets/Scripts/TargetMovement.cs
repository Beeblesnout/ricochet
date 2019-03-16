using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetMovement : MonoBehaviour
{
    Vector3 defaultPosition;
    public float xScale;
    public float yScale;
    
    void Start()
    {
        defaultPosition = transform.position;
    }

    void Update()
    {
        transform.position = defaultPosition + new Vector3(Mathf.Cos(Time.time / 2) * xScale, Mathf.Sin(Time.time) * yScale);
    }
}
