using Popcron.Console;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : SingletonBase<LevelManager>
{
    public bool hasLevelBeenLoaded = false;
    public int loadedLevel;
    public Transform player;
    public List<Transform> levels;

    public List<Transform> team1spawns;
    public Transform team1base;
    public List<Transform> team2spawns;
    public Transform team2base;
    public Transform flagLocation;

    public void LoadLevel(int l)
    {
        foreach (Transform level in levels) 
        team1spawns = new List<Transform>(levels[l].Find("red spawns").GetComponentsInChildren<Transform>());
        team1base = levels[l].Find("red base");
        team2spawns = new List<Transform>(levels[l].Find("blue spawns").GetComponentsInChildren<Transform>());
        team1base = levels[l].Find("blue base");
        flagLocation = levels[l].Find("flag location");
    }

    public Vector3 GetRandomSpawnLoc(int team)
    {
        switch(team)
        {
            case 1:
                return team1spawns[Random.Range(0, team1spawns.Count)].position;
            case 2:
                return team2spawns[Random.Range(0, team2spawns.Count)].position;
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
