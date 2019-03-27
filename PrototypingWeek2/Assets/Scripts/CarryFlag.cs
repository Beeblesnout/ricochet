using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarryFlag : MonoBehaviour
{
    [SerializeField]
    public int locationTeamBaseID;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("TeamBase"))
        {
            locationTeamBaseID = other.GetComponent<TeamBase>().baseTeamID;
            Debug.Log(locationTeamBaseID);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("TeamBase"))
        {
            locationTeamBaseID = 0;
        }
    }
}
