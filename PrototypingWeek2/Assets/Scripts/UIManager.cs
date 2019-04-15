using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using Popcron.Networking;

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

    [Header("Player References")]
    public CharacterMotion playerMotion;
    public GunController playerGun;
    public Health playerHealth;

    [Header("Announcements")]
    /// <summary>
    /// Announcement Duration
    /// </summary>
    public float annDuration;
    /// <summary>
    /// Announcement Start Time
    /// </summary>
    private float annStartTime;
    private bool announcing;

    void Start()
    {
        LocateUIElements();
    }

    public void LocateUIElements()
    {
        canvas = ((Canvas)FindObjectOfType(typeof(Canvas))).transform;
        aimCrosshair = GameObject.Find("AimCrosshair").transform;
        aimReflectLine = aimCrosshair.GetComponent<LineRenderer>();
        fpsCounter = GameObject.Find("FPSCounter").GetComponent<TMP_Text>();
        currentGunText = GameObject.Find("SelectedGun").GetComponent<TMP_Text>();
        teamText = GameObject.Find("TeamText").GetComponent<TMP_Text>();
        healthBar = GameObject.Find("HealthBar").GetComponent<Slider>();
        announcementText = GameObject.Find("AnnouncementText").GetComponent<TMP_Text>();
        announcementText.enabled = false;
    }

    public void LinkUIElements()
    {
        if (PlayerUser.Local.IsMine)
        {
            playerMotion = PlayerUser.Local.AvatarMotion;
            playerGun = PlayerUser.Local.AvatarGun;
            playerHealth = PlayerUser.Local.AvatarHealth;
        }
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
    
    public void MakeAnnouncement(string message)
    {
        annStartTime = Time.time;
        announcementText.text = message;
        announcementText.gameObject.SetActive(true);
        Debug.Log(message);
    }

    void AnnouncementUpdate()
    {
        if (announcing)
        {
            if (Time.time - annStartTime < annDuration)
            {

            }
        }
        else
        {
            announcementText.gameObject.SetActive(false);
        }
    }
}
