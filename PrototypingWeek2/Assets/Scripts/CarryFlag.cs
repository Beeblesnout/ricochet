using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarryFlag : MonoBehaviour
{
    [SerializeField]
    public int locationTeamBaseID;

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
