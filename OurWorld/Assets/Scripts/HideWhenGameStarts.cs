using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideWhenGameStarts : MonoBehaviour
{
    void Start()
    {
        GetComponent<MeshRenderer>().enabled = false;
    }
}
