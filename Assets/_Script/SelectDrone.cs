using System;
using System.Collections.Generic;
using DigitalRubyShared;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;


[RequireComponent(typeof(ARRaycastManager))]
public class SelectDrone : MonoBehaviour
{
    public GameObject[] droneModels;
    public GameObject SpawnedDrone { get; private set; }

    public GameObject droneController;
    public GameObject selectionMenu;

    static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();
    private Pose hitPose;
    private bool hasTaped;

    public static event Action onPlacedObject;

    ARRaycastManager m_RaycastManager;

    void Awake()
    {
        OnEnable();
    }

    private void OnEnable()
    {
        m_RaycastManager = GetComponent<ARRaycastManager>();
        droneController.SetActive(false);
        selectionMenu.SetActive(false);
        hasTaped = false;
    }

    bool TryGetTouchPosition(out Vector2 touchPosition)
    {

        if (Input.touchCount > 0)
        {
            touchPosition = Input.GetTouch(0).position;
            return true;
        }
        touchPosition = default;
        return false;
    }

    void Update()
    {
        if (!hasTaped)
        {
            if (!TryGetTouchPosition(out Vector2 touchPosition))
                return;

            if (m_RaycastManager.Raycast(touchPosition, s_Hits, TrackableType.PlaneWithinPolygon))
            {
                hasTaped = true;

                if (onPlacedObject != null)
                {
                    onPlacedObject();
                }

                hitPose = s_Hits[0].pose;
                selectionMenu.SetActive(true);
                
            }
        }
    }

    public void OnClickDroneSelected(int selection)
    {
        SpawnedDrone = Instantiate(droneModels[selection], hitPose.position, hitPose.rotation);

        droneController.SetActive(true);
        DemoScriptJoystick droneJoystick = droneController.GetComponent<DemoScriptJoystick>();
        droneJoystick.DroneControl = SpawnedDrone.GetComponent<CustomControllerScript>();
    }
}
