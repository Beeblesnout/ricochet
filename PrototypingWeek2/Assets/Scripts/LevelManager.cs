using Popcron.Console;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : SingletonBase<LevelManager>
{
    // TODO: Do proper level loading
    public Transform player;
    public GameObject level1;
    public GameObject level2;
    public GameObject level3;
    public Transform team1spawns;
    public Transform team2spawns;

    private void Start()
    {
        team1spawns = level1.transform.Find("red spawns");
        team2spawns = level1.transform.Find("blue spawns");
    }

    private void Update()
    {
        // load level 1
        if (Input.GetKeyDown(KeyCode.Alpha1)) 
        {
            level1.SetActive(true);
            level2.SetActive(false);
            level3.SetActive(false);
            team1spawns = level1.transform.Find("red spawns");
            team2spawns = level1.transform.Find("blue spawns");
        }

        // load level 2
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            level2.SetActive(true);
            level1.SetActive(false);
            level3.SetActive(false);
            team1spawns = level1.transform.Find("red spawns");
            team2spawns = level1.transform.Find("blue spawns");
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            level3.SetActive(true);
            level2.SetActive(false);
            level1.SetActive(false);
        }
    }

    public Vector3 GetRandomSpawnLoc(int team)
    {
        Transform[] spawns;
        switch(team)
        {
            case 1:
                spawns = team1spawns.GetComponentsInChildren<Transform>();
                return spawns[Random.Range(0, spawns.Length)].position;
            case 2:
                spawns = team1spawns.GetComponentsInChildren<Transform>();
                return spawns[Random.Range(0, spawns.Length)].position;
            default:
                Debug.Log("Invalid Team: " + team);
                return Vector3.zero;
        }
    }

    [Command("balance teams")]
    public static void BalanceTeams ()
    {
        PlayerUser[] users = FindObjectsOfType<PlayerUser>();
        if (users.Length == 0)
        {
            Debug.Log("No users, are you connected to a server?");
            return;
        }

        bool team1 = true;
        foreach (PlayerUser user in users)
        {
            user.JoinTeam(team1 ? 1 : 2);
            team1 = !team1;
        }
    }
}
