using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;

public class UIHover : MonoBehaviour
{
    [FMODUnity.EventRef]
    public string hoverUISound;

    void OnMouseEnter()
    {
        FMODUnity.RuntimeManager.PlayOneShot(hoverUISound);
    }
}
