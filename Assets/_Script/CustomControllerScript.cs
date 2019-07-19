using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomControllerScript : MonoBehaviour
{
    [SerializeField]
    DroneMovement droneMovement;

    public float Backward { get => droneMovement.customFeed_backward; set => droneMovement.customFeed_backward = value; }
    public float Forward { get => droneMovement.customFeed_forward; set => droneMovement.customFeed_forward = value; }
    public float Upward { get => droneMovement.customFeed_upward; set => droneMovement.customFeed_upward = value; }
    public float Downward { get => droneMovement.customFeed_downward; set => droneMovement.customFeed_downward = value; }
    public float Leftward { get => droneMovement.customFeed_leftward; set => droneMovement.customFeed_leftward = value; }
    public float Rightward { get => droneMovement.customFeed_rightward; set => droneMovement.customFeed_rightward = value; }
    public float Rotateleft { get => droneMovement.customFeed_rotateLeft; set => droneMovement.customFeed_rotateLeft = value; }
    public float Rotateright { get => droneMovement.customFeed_rotateRight; set => droneMovement.customFeed_rotateRight = value; }   

}
