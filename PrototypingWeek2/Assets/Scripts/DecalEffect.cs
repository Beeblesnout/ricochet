using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecalEffect : Effect
{
    public AnimationCurve scaleOverLifetime;

    public void Start() {
        duration += Random.Range(-.35f, .35f);
    }

    public new void Update()
    {
        transform.localScale = new Vector3(scaleOverLifetime.Evaluate(lifePercent), scaleOverLifetime.Evaluate(lifePercent), 1);
        Tick();
    }
}
