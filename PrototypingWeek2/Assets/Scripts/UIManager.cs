using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Console = Popcron.Console.Console;

public class UIManager : SingletonBase<UIManager>
{
    public Transform canvas;

    public Transform aimCrosshair;

    public TextMeshProUGUI dS_PointsText;
    public TextMeshProUGUI dS_ShotsText;
    public TextMeshProUGUI dS_AccuracyText;
    float dS_Points;
    float dS_Shots;
    float dS_Accuracy;
    public TextMeshProUGUI rS_PointsText;
    public TextMeshProUGUI rS_ShotsText;
    public TextMeshProUGUI rS_AccuracyText;
    float rS_Points;
    float rS_Shots;
    float rS_Accuracy;

    public TMP_Text fpsCounter;

    public override void Awake()
    {
        base.Awake();
        Console.Initialize();
    }

    void Update()
    {
        fpsCounter.text = 1f / Time.deltaTime + "";

        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();

        if (Input.GetKeyDown(KeyCode.R))
            ResetStats();
    }

    public void RicochetTargetHit(float points) 
    {
        rS_Points += points;
        rS_Shots++;
        rS_Accuracy = rS_Points / rS_Shots;

        rS_PointsText.text = rS_Points.ToString();
        rS_ShotsText.text = rS_Shots.ToString();
        rS_AccuracyText.text = rS_Accuracy.ToString();
    }

    public void DirectTargetHit(float points) 
    {
            dS_Points += points;
            dS_Shots++;
            dS_Accuracy = dS_Points / dS_Shots;

            dS_PointsText.text = dS_Points.ToString();
            dS_ShotsText.text = dS_Shots.ToString();
            dS_AccuracyText.text = dS_Accuracy.ToString();
    }

    void ResetStats() 
    {
        rS_Points = 0;
        rS_Shots = 0;
        rS_Accuracy = 0;

        rS_PointsText.text = "0";
        rS_ShotsText.text = "0";
        rS_AccuracyText.text = "0";


        dS_Points = 0;
        dS_Shots = 0;
        dS_Accuracy = 0;

        dS_PointsText.text = "0";
        dS_ShotsText.text = "0";
        dS_AccuracyText.text = "0";
    }
}
