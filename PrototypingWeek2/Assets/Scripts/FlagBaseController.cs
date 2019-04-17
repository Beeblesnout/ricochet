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
    public CarryFlagController flagController;
    [SerializeField]
    public bool flagCaptured;
    [SerializeField]
    public bool isNewFlagAvailable;

    public int team1Score, team2Score;
    public float timeLeft;
    public float roundTime;

    public TMPro.TMP_Text scoreText;

    public GameObject flagCloth;
    public GameObject carryFlagPrefab;
    public GameObject carryFlag;

    public FlagEvent onFlagCaptured;
    public FlagEvent onFlagInRightTeamBase;

    private void Start()
    {
        //Initialize flag base state.
        flagCaptured = false;
        isNewFlagAvailable = true;

        //Add listeners to flag related events.
        onFlagCaptured.AddListener(UIManager.Instance.MakeAnnouncement);
        onFlagInRightTeamBase.AddListener(UIManager.Instance.MakeAnnouncement);
        onFlagInRightTeamBase.AddListener(OnFlagScore);

        timeLeft = roundTime;
    }

    private void Update()
    {
        //Check if anyteam is scoring while a flag is being carried.
        if (flagCaptured)
        {
            CheckIsFlagInRightTeamBase();
        }

        timeLeft -= Time.deltaTime;
        scoreText.text = string.Format("{0:D}|{1:D3}|{2:D}", (int)team1Score, (int)timeLeft, (int)team2Score);
    }

    private void CheckIsFlagInRightTeamBase()
    {
        int teamID = flagCarryer.GetComponent<CharacterMotion>().teamID;
        int baseID = carryFlag.GetComponent<CarryFlag>().locationTeamBaseID;
        if (teamID == baseID)
        {
            if (teamID == 1)
            {
                onFlagInRightTeamBase.Invoke("Red Team Has Captured The Flag!");
                team1Score++;
            }
            else if (teamID == 2)
            {
                onFlagInRightTeamBase.Invoke("Blue Team Has Captured The Flag!");
                team2Score++;
            }
            else
            {
                onFlagInRightTeamBase.Invoke("How on earth have you scored with a non-existent team? That's illegal you clod.");
            }
        }
    }

    public void SetFlagCarrier(PlayerUser user)
    {
        flagCarryer = user.Avatar;
        flagController.carryer = user.Avatar;
    }

    private void CaptureFlag(Collider playerCollider)
    {
        //Update flag state.
        flagCaptured = true;
        isNewFlagAvailable = false;
        flagCloth.gameObject.SetActive(false);
        flagCarryer = playerCollider.gameObject;
        carryFlag = Instantiate(carryFlagPrefab);
        carryFlag.GetComponent<CarryFlagController>().carryer = playerCollider.gameObject;

        //Invoke event with announcement.
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
        //Return flag shortly after scoring.
        StartCoroutine("RefreshFlagState");
    }

    public void OnCarryerDie()
    {
        //Return flag shortly after dropping.
        StartCoroutine("RefreshFlagState");
        UIManager.Instance.MakeAnnouncement("The Flag Carrier Has Died!\n(Flag returning shortly)");
    }

    private IEnumerator RefreshFlagState()
    {
        //Regenerate the flag in flagbase.
        flagCarryer = null;
        flagCaptured = false;
        Destroy(carryFlag);
        yield return new WaitForSeconds(3);
        flagCloth.gameObject.SetActive(true);
        isNewFlagAvailable = true;
    }
}
