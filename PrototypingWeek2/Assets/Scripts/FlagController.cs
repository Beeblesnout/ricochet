using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FlagController : MonoBehaviour
{
    public GameObject flagCloth;
    public GameObject flagCatcher;

    public UnityEvent onFlagCaptured;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            flagCloth.gameObject.SetActive(false);
            flagCatcher = other.gameObject;
        }
    }

}
