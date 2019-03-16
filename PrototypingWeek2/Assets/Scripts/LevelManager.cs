using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : SingletonBase<LevelManager>
{
    public Transform player;
    public GameObject level1;
    public GameObject level2;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) 
        {
            level1.SetActive(true);
            level2.SetActive(false);
            player.position = Vector3.zero + Vector3.up;
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            level2.SetActive(true);
            level1.SetActive(false);
            player.position = Vector3.zero + Vector3.up;
        }
    }
    
}
