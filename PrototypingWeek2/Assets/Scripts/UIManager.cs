using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

public class UIManager : SingletonBase<UIManager>
{
    public Transform canvas;

    public Transform aimCrosshair;
    public LineRenderer aimReflectLine;
    public TMP_Text fpsCounter;
    public TMP_Text currentGunText;
    public TMP_Text teamText;
    public Slider healthBar;
    public TMP_Text announcementText;

    public CharacterMotion playerMotion;
    public GunController playerGun;
    private Health playerHealth;

    public int levelToLoad;

    public void LocateUIElements()
    {
        canvas = (Transform)FindObjectOfType(typeof(Canvas));
        aimCrosshair = GameObject.Find("AimCrosshair").transform;
        aimReflectLine = aimCrosshair.GetComponent<LineRenderer>();
        fpsCounter = GameObject.Find("FPSCounter").GetComponent<TMP_Text>();
        currentGunText = GameObject.Find("SelectedGun").GetComponent<TMP_Text>();
        teamText = GameObject.Find("TeamText").GetComponent<TMP_Text>();
        healthBar = GameObject.Find("HealthBar").GetComponent<Slider>();
        announcementText = GameObject.Find("AnnouncementText").GetComponent<TMP_Text>();
    }

    public void LinkUIElements(CharacterMotion newMotion)
    {
        playerMotion = newMotion;
        if (playerGun == null) playerGun = playerMotion.gameObject.GetComponentInChildren<GunController>();
        if (playerHealth == null) playerHealth = playerMotion.gameObject.GetComponent<Health>();
    }

    void Update()
    {
        fpsCounter.text = 1f / Time.deltaTime + "";

        if (playerGun)
        {
            SetCrosshairPos();
            currentGunText.text = playerGun.gun.gunName;
        }
        if (playerHealth) healthBar.value = Mathf.Clamp01(playerHealth.displayHealth / playerHealth.maxHealth);

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
            line.SetPosition(i, point + dir * i * ricochetLineDistanceCurve.Evaluate(dist / 2) * 3);
        }
    }
    
    public async void Announcement(string message)
    {
        announcementText.text = message;
        Debug.Log(message);
        if (announcementText.gameObject.activeInHierarchy)
        {
            announcementText.gameObject.SetActive(true);
            await Task.Delay(3000);
            announcementText.gameObject.SetActive(false);
        }
    }
}
