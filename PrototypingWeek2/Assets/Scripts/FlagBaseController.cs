using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//This event will be invoked once the flag is captured, containing the playerTeamID.
[System.Serializable]
public class FlagEvent : UnityEvent<string> { }

public class FlagBaseController : MonoBehaviour
{
    [SerializeField]
    public GameObject flagCarryer;
    [SerializeField]
    public bool flagCaptured;
    [SerializeField]
    public bool isNewFlagAvailable;

    public int team1Score, team2Score;
    public float timeLeft;
    public float roundTime;

    public GameObject flagCloth;
    public GameObject carryFlagPrefab;
    public GameObject carryFlag;

    public FlagEvent onFlagCaptured;
    public FlagEvent onFlagInRightTeamBase;

    private void Start()
    {
        flagCaptured = false;
        isNewFlagAvailable = true;
        onFlagCaptured.AddListener(UIManager.Instance.Announcement);
        onFlagInRightTeamBase.AddListener(UIManager.Instance.Announcement);
        onFlagInRightTeamBase.AddListener(OnFlagScore);
    }

    private void Update()
    {
        if (flagCaptured)
        {
            CheckIsFlagInRightTeamBase();
        }
    }

    private void CheckIsFlagInRightTeamBase()
    {
        int teamID = flagCarryer.GetComponent<CharacterMotion>().teamID;
        int baseID = carryFlag.GetComponent<CarryFlag>().locationTeamBaseID;
        if (teamID == baseID)
        {
            if (teamID == 1)
            {
                onFlagCaptured.Invoke("Red Team Has Captured The Flag!");
                team1Score++;
            }
            else if (teamID == 2)
            {
                onFlagCaptured.Invoke("Blue Team Has Captured The Flag!");
                team2Score++;
            }
            else
            {
                onFlagCaptured.Invoke("How on earth have you scored with a non-existent team? That's illegal you clod.");
            }
        }
    }

    private void CaptureFlag(Collider playerCollider)
    {
        flagCaptured = true;
        isNewFlagAvailable = false;
        flagCloth.gameObject.SetActive(false);
        flagCarryer = playerCollider.gameObject;
        Debug.Log(flagCarryer.name);
        carryFlag = Instantiate(carryFlagPrefab);
        carryFlag.GetComponent<CarryFlagController>().carryer = playerCollider.gameObject;
        int teamID = flagCarryer.GetComponent<CharacterMotion>().player.user.teamID;
        if (teamID == 1)
        {
            onFlagCaptured.Invoke("Red Team Has Picked Up The Flag!");
        }
        else if (teamID == 2)
        {
            onFlagCaptured.Invoke("Blue Team Has Picked Up The Flag!");
        }
        else
        {
            onFlagCaptured.Invoke("Excuse me? You're not on a right team! Drop that flag right now!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player" && isNewFlagAvailable)
        {
            CaptureFlag(other);
        }
    }

    public void OnFlagScore(string message)
    {
        StartCoroutine("RefreshFlagState");
    }

    public void OnCarryerDie()
    {
        StartCoroutine("RefreshFlagState");
        UIManager.Instance.Announcement("The Flag Carrier Has Died!\n(Flag returning shortly)");
    }

     private IEnumerator RefreshFlagState()
    {
        flagCarryer = null;
        flagCaptured = false;
        Destroy(carryFlag);
        yield return new WaitForSeconds(5);
        flagCloth.gameObject.SetActive(true);
        isNewFlagAvailable = true;
    }
}
