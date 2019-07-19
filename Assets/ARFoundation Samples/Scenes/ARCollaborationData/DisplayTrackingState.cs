using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

/// <summary>
/// When relocalizing with ARCollaborationData or ARWorldMaps, the tracking state
/// should change to TrackingState.Limited until the device has successfully
/// relocalized to the new data. If it remains TrackingState.Tracking, then
/// it is not working.
/// </summary>
[RequireComponent(typeof(ARSession))]
public class DisplayTrackingState : MonoBehaviour
{
    public Text m_Text;
    public ARSession m_Session;

    
    void Update()
    {
        m_Text.text = $"Session ID = {m_Session.subsystem.sessionId}\nSession state = {ARSession.state.ToString()}\nTracking state = {m_Session.subsystem.trackingState}";
    }
}
