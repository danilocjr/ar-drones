using System;
using System.Collections;
using System.Collections.Generic;
using DigitalRubyShared;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;


public class SelectDroneFromMarker : MonoBehaviour
{

    public ARTrackedImageManager m_TrackedImageManager;

    public GameObject[] droneModels;
    public GameObject SpawnedDrone { get; private set; }
    Transform droneBaseParent;

    public GameObject droneController;
    public GameObject selectionMenu;

    public Text info;

    void OnEnable()
    {
        m_TrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        m_TrackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var trackedImage in eventArgs.added)
            droneBaseParent = trackedImage.transform;

        foreach (var trackedImage in eventArgs.updated)
            droneBaseParent = trackedImage.transform;

        foreach (var trackedImage in eventArgs.removed)
            droneBaseParent = null;
    }
   
    private void Start()
    {
        droneController.SetActive(false);
        selectionMenu.SetActive(true);
    }

    IEnumerator BaseNotFound()
    {
        yield return new WaitForSeconds(2);
        info.text = "";
    }

    public void OnClickDroneSelected(int selection)
    {
        if (droneBaseParent == null)
        {
            info.text = "Are you seeing the DRONE BASE?";
            StopCoroutine("BaseNotFound");
            StartCoroutine("BaseNotFound");
            return;
        }

        if (SpawnedDrone != null)
            Destroy(SpawnedDrone);

        droneBaseParent.position = new Vector3(droneBaseParent.position.x, droneBaseParent.position.y + 0.095f, droneBaseParent.position.z);
        SpawnedDrone = Instantiate(droneModels[selection], droneBaseParent.position, droneBaseParent.rotation);

        info.text = "Click to fly, dude!";
        StopCoroutine("BaseNotFound");
        StartCoroutine("BaseNotFound");
    }

    public void OnClickFly()
    {
        if(SpawnedDrone == null)
        {
            info.text = "Need to select a DRONE";
            StopCoroutine("BaseNotFound");
            StartCoroutine("BaseNotFound");
            return;
        }

        selectionMenu.SetActive(false);

        droneController.SetActive(true);
        DemoScriptJoystick droneJoystick = droneController.GetComponent<DemoScriptJoystick>();
        droneJoystick.DroneControl = SpawnedDrone.GetComponent<CustomControllerScript>();
    }

    public void OnClickRestart()
    {
        if (SpawnedDrone != null)
            Destroy(SpawnedDrone);

        droneController.SetActive(false);
        selectionMenu.SetActive(true);
    }
   
}
