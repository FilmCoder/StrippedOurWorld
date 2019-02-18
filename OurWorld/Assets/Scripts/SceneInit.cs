using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneInit : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //UnityEditor.PlayerSettings.
        //UnityEngine.VR.VRSettings.LoadDeviceByName("OpenVR")        Debug.Log("Loading OpenVR");
        UnityEngine.XR.XRSettings.LoadDeviceByName("OpenVR");
        Debug.Log("Finished Loading OpenVR");


    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
