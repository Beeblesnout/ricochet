using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarryFlag : MonoBehaviour
{
    //To store the current flag carrier's team ID.
    public int carrierID;

    //To store the team ID of the location player is standing on.
    [SerializeField]
    public int locationTeamBaseID;

    //Detect and update the location teambase ID.
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "TeamBase")
        {
            locationTeamBaseID = other.GetComponent<TeamBase>().baseTeamID;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "TeamBase")
        {
            locationTeamBaseID = 0;
        }
    }
}
