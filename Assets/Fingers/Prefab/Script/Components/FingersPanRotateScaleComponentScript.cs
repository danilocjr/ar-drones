//
// Fingers Gestures
// (c) 2015 Digital Ruby, LLC
// http://www.digitalruby.com
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 

using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DigitalRubyShared
{
    /// <summary>
    /// Allows two finger pan, scale and rotate on a game object
    /// </summary>
    [AddComponentMenu("Fingers Gestures/Component/Fingers Pan Rotate Scale", 4)]
    public class FingersPanRotateScaleComponentScript : MonoBehaviour
    {
        public enum _DoubleTapResetMode
        {
            Off = 0,
            ResetScaleRotation = 1,
            ResetScaleRotationPosition = 2
        }

        [Header("Setup")]
        [Tooltip("The cameras to use to convert screen coordinates to world coordinates. Defaults to Camera.main.")]
        public Camera[] Cameras;

        [Tooltip("Whether to bring the object to the front when a gesture executes on it")]
        public bool BringToFront = true;

        [Tooltip("Whether the gestures in this script can execute simultaneously with all other gestures.")]
        public bool AllowExecutionWithAllGestures;

        [Tooltip("The mode to execute in, can be require over game object or allow on any game object.")]
        public GestureRecognizerComponentScriptBase.GestureObjectMode Mode = GestureRecognizerComponentScriptBase.GestureObjectMode.RequireIntersectWithGameObject;

        [Tooltip("The minimum and maximum scale. 0 for no limits. 1 for no scaling.")]
        public Vector2 MinMaxScale;

        [Header("Enable / Disable Gestures")]
        [Tooltip("Whether to add a double tap to reset the transform of the game object this script is on. This must be set in the inspector and not changed.")]
        public _DoubleTapResetMode DoubleTapResetMode;

        [Tooltip("Whether to allow panning. Can be set during editor or runtime.")]
        public bool AllowPan = true;

        [Tooltip("Whether to allow scaling. Can be set during editor or runtime.")]
        public bool AllowScale = true;

        [Tooltip("Whether to allow rotating. Can be set during editor or runtime.")]
        public bool AllowRotate = true;

        /// <summary>
        /// Allow moving the target
        /// </summary>
        public PanGestureRecognizer PanGesture { get; private set; }

        /// <summary>
        /// Allow scaling the target
        /// </summary>
        public ScaleGestureRecognizer ScaleGesture { get; private set; }

        /// <summary>
        /// Allow rotating the target
        /// </summary>
        public RotateGestureRecognizer RotateGesture { get; private set; }

        /// <summary>
        /// The double tap gesture or null if DoubleTapToReset was false when this script started up
        /// </summary>
        public TapGestureRecognizer DoubleTapGesture { get; private set; }

        private Rigidbody2D rigidBody2D;
        private Rigidbody rigidBody;
        private SpriteRenderer spriteRenderer;
        private CanvasRenderer canvasRenderer;
        private Transform _transform;
        private int startSortOrder;
        private float panZ;
        private Vector3 panOffset;

        private struct SavedState
        {
            public Vector3 Scale;
            public Quaternion Rotation;
            public Vector3 Position;
        }
        private readonly Dictionary<Transform, SavedState> savedStates = new Dictionary<Transform, SavedState>();

        private void PanGestureUpdated(DigitalRubyShared.GestureRecognizer r)
        {
            if (!AllowPan)
            {
                r.Reset();
                return;
            }

            Camera camera;
            GameObject obj = FingersScript.StartOrResetGesture(r, BringToFront, Cameras, gameObject, spriteRenderer, Mode, out camera);
            if (camera == null)
            {
                r.Reset();
                return;
            }
            else if (r.State == GestureRecognizerState.Began)
            {
                SetStartState(r, obj, false);
            }
            else if (r.State == GestureRecognizerState.Executing && _transform != null)
            {
                if (PanGesture.ReceivedAdditionalTouches)
                {
                    panZ = camera.WorldToScreenPoint(_transform.position).z;
                    panOffset = _transform.position - camera.ScreenToWorldPoint(new Vector3(r.FocusX, r.FocusY, panZ));
                }
                Vector3 gestureScreenPoint = new Vector3(r.FocusX, r.FocusY, panZ);
                Vector3 gestureWorldPoint = camera.ScreenToWorldPoint(gestureScreenPoint) + panOffset;
                if (rigidBody != null)
                {
                    rigidBody.MovePosition(gestureWorldPoint);
                }
                else if (rigidBody2D != null)
                {
                    rigidBody2D.MovePosition(gestureWorldPoint);
                }
                else if (canvasRenderer != null)
                {
                    _transform.position = gestureScreenPoint;
                }
                else
                {
                    _transform.position = gestureWorldPoint;
                }
            }
            else if (r.State == GestureRecognizerState.Ended)
            {
                if (spriteRenderer != null && BringToFront)
                {
                    spriteRenderer.sortingOrder = startSortOrder;
                }
                ClearStartState();
            }
        }

        private void ScaleGestureUpdated(DigitalRubyShared.GestureRecognizer r)
        {
            if (!AllowScale)
            {
                r.Reset();
                return;
            }
            else if (MinMaxScale.x == MinMaxScale.y && MinMaxScale.y == 1.0f)
            {
                return;
            }

            Camera camera;
            GameObject obj = FingersScript.StartOrResetGesture(r, BringToFront, Cameras, gameObject, spriteRenderer, Mode, out camera);
            if (camera == null)
            {
                r.Reset();
                return;
            }
            else if (r.State == GestureRecognizerState.Began)
            {
                SetStartState(r, obj, false);
            }
            else if (r.State == GestureRecognizerState.Executing && _transform != null)
            {
                // assume uniform scale
                float scale = _transform.localScale.x * ScaleGesture.ScaleMultiplier;

                if (MinMaxScale.x > 0.0f && MinMaxScale.y >= MinMaxScale.x)
                {
                    scale = Mathf.Clamp(scale, MinMaxScale.x, MinMaxScale.y);
                }

                // don't mess with z scale for 2D
                float zScale = (rigidBody2D == null && spriteRenderer == null && canvasRenderer == null ? scale : _transform.localScale.z);
                _transform.localScale = new Vector3(scale, scale, zScale);
            }
            else if (r.State == GestureRecognizerState.Ended)
            {
                ClearStartState();
            }
        }

        private void RotateGestureUpdated(DigitalRubyShared.GestureRecognizer r)
        {
            if (!AllowRotate)
            {
                r.Reset();
                return;
            }

            Camera camera;
            GameObject obj = FingersScript.StartOrResetGesture(r, BringToFront, Cameras, gameObject, spriteRenderer, Mode, out camera);
            if (camera == null)
            {
                r.Reset();
                return;
            }
            else if (r.State == GestureRecognizerState.Began)
            {
                SetStartState(r, obj, false);
            }
            else if (r.State == GestureRecognizerState.Executing && _transform != null)
            {
                if (rigidBody != null)
                {
                    float angle = RotateGesture.RotationDegreesDelta;
                    Quaternion rotation = Quaternion.AngleAxis(angle, camera.transform.forward);
                    rigidBody.MoveRotation(rigidBody.rotation * rotation);
                }
                else if (rigidBody2D != null)
                {
                    rigidBody2D.MoveRotation(rigidBody2D.rotation + RotateGesture.RotationDegreesDelta);
                }
                else if (canvasRenderer != null)
                {
                    _transform.Rotate(Vector3.forward, RotateGesture.RotationDegreesDelta, Space.Self);
                }
                else
                {
                    _transform.Rotate(camera.transform.forward, RotateGesture.RotationDegreesDelta, Space.Self);
                }
            }
            else if (r.State == GestureRecognizerState.Ended)
            {
                ClearStartState();
            }
        }

        private void DoubleTapGestureUpdated(DigitalRubyShared.GestureRecognizer r)
        {
            if (DoubleTapResetMode == _DoubleTapResetMode.Off)
            {
                r.Reset();
                return;
            }
            else if (r.State == GestureRecognizerState.Ended)
            {
                Camera camera = FingersScript.GetCameraForGesture(r, Cameras);
                GameObject obj = FingersScript.GestureIntersectsObject(r, camera, gameObject, Mode);
                SavedState state;
                if (obj != null && savedStates.TryGetValue(obj.transform, out state))
                {
                    obj.transform.rotation = state.Rotation;
                    obj.transform.localScale = state.Scale;
                    if (DoubleTapResetMode == _DoubleTapResetMode.ResetScaleRotationPosition)
                    {
                        obj.transform.position = state.Position;
                    }
                    savedStates.Remove(obj.transform);
                }
            }
        }

        private void ClearStartState()
        {
            if (Mode != GestureRecognizerComponentScriptBase.GestureObjectMode.AllowOnAnyGameObjectViaRaycast)
            {
                return;
            }
            else if (PanGesture.State != GestureRecognizerState.Executing &&
                RotateGesture.State != GestureRecognizerState.Executing &&
                ScaleGesture.State != GestureRecognizerState.Executing)
            {
                rigidBody2D = null;
                rigidBody = null;
                spriteRenderer = null;
                canvasRenderer = null;
                _transform = null;
            }
        }

        private bool SetStartState(DigitalRubyShared.GestureRecognizer gesture, GameObject obj, bool force)
        {
            if (!force && Mode != GestureRecognizerComponentScriptBase.GestureObjectMode.AllowOnAnyGameObjectViaRaycast)
            {
                return false;
            }
            else if (obj == null)
            {
                ClearStartState();
                return false;
            }
            else if (_transform == null)
            {
                rigidBody2D = obj.GetComponent<Rigidbody2D>();
                rigidBody = obj.GetComponent<Rigidbody>();
                spriteRenderer = obj.GetComponent<SpriteRenderer>();
                canvasRenderer = obj.GetComponent<CanvasRenderer>();
                if (spriteRenderer != null)
                {
                    startSortOrder = spriteRenderer.sortingOrder;
                }
                _transform = (rigidBody == null ? (rigidBody2D == null ? obj.transform : rigidBody2D.transform) : rigidBody.transform);
                if (DoubleTapResetMode != _DoubleTapResetMode.Off && !savedStates.ContainsKey(_transform))
                {
                    savedStates[_transform] = new SavedState { Rotation = _transform.rotation, Scale = _transform.localScale, Position = _transform.position };
                }
            }
            else if (_transform != obj.transform)
            {
                if (gesture != null)
                {
                    gesture.Reset();
                }
                return false;
            }
            return true;
        }

        private void OnEnable()
        {
            if ((Cameras == null || Cameras.Length == 0) && Camera.main != null)
            {
                Cameras = new Camera[] { Camera.main };
            }
            PanGesture = new PanGestureRecognizer { MaximumNumberOfTouchesToTrack = 2, ThresholdUnits = 0.01f };
            PanGesture.StateUpdated += PanGestureUpdated;
            ScaleGesture = new ScaleGestureRecognizer();
            ScaleGesture.StateUpdated += ScaleGestureUpdated;
            RotateGesture = new RotateGestureRecognizer();
            RotateGesture.StateUpdated += RotateGestureUpdated;
            if (Mode != GestureRecognizerComponentScriptBase.GestureObjectMode.AllowOnAnyGameObjectViaRaycast)
            {
                SetStartState(null, gameObject, true);
            }
            if (DoubleTapResetMode != _DoubleTapResetMode.Off)
            {
                DoubleTapGesture = new TapGestureRecognizer { NumberOfTapsRequired = 2 };
                DoubleTapGesture.StateUpdated += DoubleTapGestureUpdated;
            }
            if (AllowExecutionWithAllGestures)
            {
                PanGesture.AllowSimultaneousExecutionWithAllGestures();
                RotateGesture.AllowSimultaneousExecutionWithAllGestures();
                ScaleGesture.AllowSimultaneousExecutionWithAllGestures();
                if (DoubleTapGesture != null)
                {
                    DoubleTapGesture.AllowSimultaneousExecutionWithAllGestures();
                }
            }
            else
            {
                PanGesture.AllowSimultaneousExecution(ScaleGesture);
                PanGesture.AllowSimultaneousExecution(RotateGesture);
                ScaleGesture.AllowSimultaneousExecution(RotateGesture);
                if (DoubleTapGesture != null)
                {
                    DoubleTapGesture.AllowSimultaneousExecution(ScaleGesture);
                    DoubleTapGesture.AllowSimultaneousExecution(RotateGesture);
                    DoubleTapGesture.AllowSimultaneousExecution(PanGesture);
                }
            }
            if (Mode == GestureRecognizerComponentScriptBase.GestureObjectMode.RequireIntersectWithGameObject)
            {
                RotateGesture.PlatformSpecificView = gameObject;
                PanGesture.PlatformSpecificView = gameObject;
                ScaleGesture.PlatformSpecificView = gameObject;
                if (DoubleTapGesture != null)
                {
                    DoubleTapGesture.PlatformSpecificView = gameObject;
                }
            }
            FingersScript.Instance.AddGesture(PanGesture);
            FingersScript.Instance.AddGesture(ScaleGesture);
            FingersScript.Instance.AddGesture(RotateGesture);
            FingersScript.Instance.AddGesture(DoubleTapGesture);
        }

        private void OnDisable()
        {
            if (FingersScript.HasInstance)
            {
                FingersScript.Instance.RemoveGesture(PanGesture);
                FingersScript.Instance.RemoveGesture(ScaleGesture);
                FingersScript.Instance.RemoveGesture(RotateGesture);
                FingersScript.Instance.RemoveGesture(DoubleTapGesture);
            }
        }
    }
}
