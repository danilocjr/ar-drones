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

        private void JoystickLeftExecuted(FingersJoystickScript script, Vector2 amount)
        {
            if (DroneControl != null)
            {
                DroneControl.Rightward = -amount.x * SpeedLeft;
                DroneControl.Leftward = amount.x * SpeedLeft;

                DroneControl.Rotateright = amount.x * SpeedLeft * 15;
                DroneControl.Rotateleft = -amount.x * SpeedLeft * 15;

                DroneControl.Forward = -amount.y * SpeedLeft;
                DroneControl.Backward = amount.y * SpeedLeft;
            }
        }


        private void JoystickRightExecuted(FingersJoystickScript script, Vector2 amount)
        {
            if(DroneControl != null)
            {               
                DroneControl.Upward = amount.y * SpeedRight;
                DroneControl.Downward = -amount.y * SpeedRight / 100f;

                
            } 
        }

    }
}
