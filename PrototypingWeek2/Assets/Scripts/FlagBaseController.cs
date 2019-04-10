using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//This event will be invoked once the flag is captured, containing the playerTeamID.
[System.Serializable]
public class FlagEvent : UnityEvent<int> { }

public class FlagBaseController : MonoBehaviour
{
    [SerializeField]
    public GameObject flagCarryer;
    [SerializeField]
    public bool flagCaptured;
    [SerializeField]
    public bool isNewFlagAvailable;

    public GameObject flagCloth;
    public GameObject carryFlagPrefab;
    public GameObject carryFlag;

    public FlagEvent onFlagCaptured;
    public FlagEvent onFlagInRightTeamBase;

    private void Start()
    {
        flagCaptured = false;
        isNewFlagAvailable = true;
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
        if (flagCarryer.GetComponent<CharacterMotion>().playerTeamID == carryFlag.GetComponent<CarryFlag>().locationTeamBaseID)
        {
            onFlagInRightTeamBase.Invoke(flagCarryer.GetComponent<CharacterMotion>().playerTeamID);
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
        onFlagCaptured.Invoke(flagCarryer.GetComponent<CharacterMotion>().playerTeamID);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player" && isNewFlagAvailable)
        {
            CaptureFlag(other);
        }
    }

    public void OnFlagScore(int teamID)
    {
        StartCoroutine("RefreshFlagState");
        Debug.Log("Team " + teamID + " score!");
    }

    public void OnCarryerDie()
    {
        StartCoroutine("RefreshFlagState");
        Debug.Log("Flag returning.");
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
