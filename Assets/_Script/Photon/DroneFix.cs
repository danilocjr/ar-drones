using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class DroneFix : MonoBehaviour
{

    PhotonView PV;
    DroneMovement drone;
    DronePropelers prop;
    CustomControllerScript controller;


    // Start is called before the first frame update
    void Start()
    {
        PV = GetComponent<PhotonView>();
        drone = GetComponent<DroneMovement>();
        prop = GetComponent<DronePropelers>();
        controller = GetComponent<CustomControllerScript>();

        if (!PV.IsMine){
            drone.enabled = false;
            prop.enabled = false;
            controller.enabled = false;
        }
    }


}
