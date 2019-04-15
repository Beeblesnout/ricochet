using Popcron.Console;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelManager : SingletonBase<LevelManager>
{
    public List<Level> levels;

    public static bool hasLoadedLevels = false;
    public int loadedLevel;
    public Transform player;

    void Start()
    {
        LoadLevels();
    }

    public void LoadLevels()
    {
        List<GameObject> foundLevels = GameObject.FindGameObjectsWithTag("Level").ToList();
        levels = foundLevels.ConvertAll<Level>(l => new Level(l.transform));

        SetLevel(0);
    }

    public void SetLevel(int index)
    {
        levels.ForEach(l => l.Deactivate());
        levels[index].Activate();
        loadedLevel = index;
    }

    public Vector3 GetRandomSpawnLoc(int team)
    {
        if (hasLoadedLevels) 
        {
            return levels[loadedLevel].GetRandomSpawn(team);
        }
        else
        {
            Debug.LogWarning("Levels have not been loaded yet.");
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

public struct Level
{
    public Transform root;
    public Transform[] team1spawns;
    public Transform[] team2spawns;
    public Transform team1base;
    public Transform team2base;
    public Transform flagLocation;

    public bool active { get; private set; }

    public Level(Transform root)
    {
        this.root = root;
        Transform l = root.Find("map locations");
        team1spawns = l.GetChild(0).GetComponentsInChildren<Transform>();
        team2spawns = l.GetChild(1).GetComponentsInChildren<Transform>();
        team1base = l.GetChild(2);
        team2base = l.GetChild(3);
        flagLocation = l.GetChild(4);
        active = root.gameObject.activeInHierarchy;
    }

    public void Activate()
    {
        active = true;
        root.gameObject.SetActive(true);
    }

    public void Deactivate()
    {
        active = false;
        root.gameObject.SetActive(false);
    }

    public Vector3 GetRandomSpawn(int team)
    {
        switch(team)
        {
            case 1:
                return team1spawns[Random.Range(0, team1spawns.Length)].position;
            case 2:
                return team2spawns[Random.Range(0, team2spawns.Length)].position;
            default:
                Debug.Log("Invalid Team: " + team);
                return Vector3.zero;
        }
    }
}
