using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DigitalRubyShared
{
    public class DemoScriptJoystick : MonoBehaviour
    {
        public FingersJoystickScript joystickRight;
        public FingersJoystickScript joystickLeft;
        public float SpeedRight = 1.0f;
        public float SpeedLeft = 1.0f;

        CustomControllerScript droneControl;
        public CustomControllerScript DroneControl { private get => droneControl; set => droneControl = value; }

        private void Awake()
        {
            joystickRight.JoystickExecuted = JoystickRightExecuted;
            joystickLeft.JoystickExecuted = JoystickLeftExecuted;
        }

        private void JoystickRightExecuted(FingersJoystickScript script, Vector2 amount)
        {
            if(DroneControl != null)
            {               
                DroneControl.Upward = amount.y * SpeedRight;
                DroneControl.Downward = -amount.y * SpeedRight / 2f;
            } 
        }
        private void JoystickLeftExecuted(FingersJoystickScript script, Vector2 amount)
        {
            if (DroneControl != null)
            {
                DroneControl.Rightward = amount.x * SpeedLeft;
                DroneControl.Leftward = -amount.x * SpeedLeft;

                DroneControl.Rotateright = amount.x * SpeedLeft;
                DroneControl.Rotateleft = -amount.x * SpeedLeft;

                DroneControl.Forward = -amount.y * SpeedLeft;
                DroneControl.Backward = amount.y * SpeedLeft;
            }
        }
    }
}
