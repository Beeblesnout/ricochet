using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
// using Popcron.console;
// using Console = Console.Console;

public class UIManager : SingletonBase<UIManager>
{
    public Transform canvas;

    public Transform aimCrosshair;
    public LineRenderer aimReflectLine;
    public TMP_Text fpsCounter;
    public TMP_Text currentGunText;

    public CharacterMotion playerMotion;
    public GunController playerGun;
    private Health playerHealth;

    public override void Awake()
    {
        base.Awake();
        // Console.Initialize();
        if (aimReflectLine == null) aimReflectLine = aimCrosshair.GetComponent<LineRenderer>();
        if (playerGun == null) playerGun = playerMotion.gameObject.GetComponentInChildren<GunController>();
        if (playerHealth == null) playerHealth = playerMotion.gameObject.GetComponent<Health>();
    }

    void Update()
    {
        fpsCounter.text = 1f / Time.deltaTime + "";

        SetCrosshairPos();

        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
    }

    void SetCrosshairPos()
    {
        RaycastHit aimHit;
        if (Physics.Raycast(playerGun.transform.position, playerGun.transform.forward, out aimHit, playerGun.gun.shotRange))
        {
            aimCrosshair.position = Camera.main.WorldToScreenPoint(aimHit.point);
            SetRicochetLine(aimHit.point, Vector3.Reflect(playerGun.transform.forward, aimHit.normal), aimHit.distance, 11);
        }
        else
        {
            aimCrosshair.position = new Vector3(Screen.width / 2, Screen.height / 2);
            SetRicochetLine();
        }
    }

    void SetRicochetLine()
    {
        LineRenderer line = aimCrosshair.GetComponent<LineRenderer>();
        line.positionCount = 0;
    }

    public AnimationCurve ricochetLineDistanceCurve;
    void SetRicochetLine(Vector3 point, Vector3 dir, float dist, int pointCount)
    {
        LineRenderer line = aimCrosshair.GetComponent<LineRenderer>();
        line.positionCount = pointCount;
        line.widthMultiplier = .025f + ricochetLineDistanceCurve.Evaluate(dist / 100);
        Vector3[] pos = new Vector3[pointCount];
        for (int i = 0; i < line.GetPositions(pos); i++)
        {
            line.SetPosition(i, point + dir * i * ricochetLineDistanceCurve.Evaluate(dist / 75) * 3);
        }
    }
}
