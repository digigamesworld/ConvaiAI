using System;
using Convai.Scripts.Runtime.LoggerSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using Inputs;
using UnityEngine.UI;
namespace Convai.Scripts.Runtime.Core
{
    [DefaultExecutionOrder(-105)]
    public class ConvaiInputManager : MonoBehaviour

    {
        [HideInInspector] public Vector2 moveVector;
        [HideInInspector] public Vector2 lookVector;

        [SerializeField] private Joystick moveBtn;
        [SerializeField] private Button talkButton;
        [SerializeField] private Button runBtn;
        [SerializeField] private Button jumpBtn;
        public bool isRunning { get; private set; }

        public Action jumping;
        public Action sendText;
        public Action toggleChat;
        public Action toggleSettings;

        public bool IsTalkKeyHeld { get; private set; }
        public Action<bool> talkKeyInteract;


        [Serializable]
        public class MovementKeys
        {
            public const KeyCode Forward = KeyCode.W;
            public const KeyCode Backward = KeyCode.S;
            public const KeyCode Right = KeyCode.D;
            public const KeyCode Left = KeyCode.A;
        }

        public KeyCode TextSendKey = KeyCode.Return;
        public KeyCode TextSendAltKey = KeyCode.KeypadEnter;
        public KeyCode TalkKey = KeyCode.T;
        public KeyCode OpenSettingPanelKey = KeyCode.F10;
        public KeyCode RunKey = KeyCode.LeftShift;
        public MovementKeys movementKeys;

        private int activeJoystickFingerId = -1;
        private int activeRotationFingerId = -1;
        [SerializeField] private float xSpeed = 120.0f;

        [SerializeField] private float touchZoomSpeed = 30.0f;
        [SerializeField] private float yMinLimit = -20f;
        [SerializeField] private float yMaxLimit = 80f;
        [SerializeField] private float fovMin = 30f;
        [SerializeField] private float minimumPitchAngle = -75;
        [SerializeField] private float maximumPitchAngle = 75;
        [SerializeField] private float rotationSpeed = 200;
        [SerializeField] private float zoomFov = 30;
        [SerializeField] private float fovMax = 60f;
        private bool isTalking;
        private bool isTexting;
        public bool WasTalkKeyPressed()
        {
            return Input.GetKeyDown(TalkKey);
        }


        public static ConvaiInputManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                ConvaiLogger.DebugLog("There's more than one ConvaiInputManager! " + transform + " - " + Instance, ConvaiLogger.LogCategory.UI);
                Destroy(gameObject);
                return;
            }

            Instance = this;
            LockCursor(false);
        }

        void Start()
        {
            // Add listeners for button events
            EventTrigger trigger = talkButton.gameObject.AddComponent<EventTrigger>();
            var pointerDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            pointerDown.callback.AddListener((_) => OnTalkButtonPress());
            trigger.triggers.Add(pointerDown);

            var pointerUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
            pointerUp.callback.AddListener((_) => OnTalkButtonRelease());
            trigger.triggers.Add(pointerUp);
        }

        private void OnTalkButtonPress()
        {
            IsTalkKeyHeld = true;
            talkKeyInteract?.Invoke(true); // Start talking action
        }

        private void OnTalkButtonRelease()
        {
            IsTalkKeyHeld = false;
            talkKeyInteract?.Invoke(false); // Stop talking action
        }
        public void OnTextButtonPress()
        {
            isTexting = true;
            // Any other logic for when the button is pressed
        }

        public void OnTextButtonRelease()
        {
            isTexting = false;
            // Any other logic for when the button is released
        }
        private void Update()
        {

            if (Input.GetButton("Jump"))
            {
                jumping?.Invoke();
            }

            moveVector = Vector2.zero;
            if (Input.GetKey(MovementKeys.Forward)) moveVector.y += 1f;
            if (Input.GetKey(MovementKeys.Backward)) moveVector.y -= 1f;
            if (Input.GetKey(MovementKeys.Left)) moveVector.x -= 1f;
            if (Input.GetKey(MovementKeys.Right)) moveVector.x += 1f;
            if (moveBtn.Direction.magnitude > 0)
                moveVector = moveBtn.Direction;
            lookVector.x = Input.GetAxis("Mouse X") * 2f;
            lookVector.y = Input.GetAxis("Mouse Y") * 2f;

#if !UNITY_EDITOR && !UNITY_STANDALONE
   var touchCount = Input.touchCount;
        if(touchCount == 0)
        {
            activeRotationFingerId = -1;
            activeJoystickFingerId = -1;
        }
if (touchCount > 0)
{
    for (int i = 0; i < touchCount; i++)
    {
        Touch touch = Input.GetTouch(i);

        if (activeJoystickFingerId == -1)
        {
            // If there's no active joystick touch, check for a new joystick touch
            if (touch.phase == TouchPhase.Began && moveBtn.Direction.magnitude > 0)
            {
               if(activeRotationFingerId != touch.fingerId )
                  activeJoystickFingerId = touch.fingerId;  // Set active joystick touch ID
            }
        }

        // If this touch is controlling the joystick
        if (touch.fingerId == activeJoystickFingerId)
        {
            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                // Reset joystick touch ID when touch ends
                activeJoystickFingerId = -1;
            }

        }
         
                    // Handle camera rotation if touch is outside joystick and not part of UI
                    if (touch.fingerId != activeJoystickFingerId)
        {
              activeRotationFingerId = touch.fingerId;
                        lookVector.y = touch.deltaPosition.y * rotationSpeed * 1 / 120 * Time.deltaTime;
                        lookVector.x = touch.deltaPosition.x * rotationSpeed * 1 / 120 * Time.deltaTime;
        }
                // If this touch is controlling the rotation
                if (touch.fingerId == activeRotationFingerId)
                {
                    if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    {
                        // Reset joystick touch ID when touch ends
                        activeRotationFingerId = -1;
                    }
                }
            }

}

#endif
            // if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject()) LockCursor(true);
            if (Input.GetKeyDown(RunKey)) isRunning = !isRunning;
            if (Input.GetKeyDown(TextSendKey) || Input.GetKeyDown(TextSendAltKey) || isTexting) sendText?.Invoke();
            if (Input.GetKeyDown(OpenSettingPanelKey)) toggleSettings?.Invoke();
            if (Input.GetKeyDown(TalkKey) || isTalking)
            {
                talkKeyInteract?.Invoke(true);
                IsTalkKeyHeld = true;
            }

            if (Input.GetKeyUp(TalkKey))
            {
                talkKeyInteract?.Invoke(false);
                IsTalkKeyHeld = false;
            }

        }

        private static void LockCursor(bool lockState)
        {
            Cursor.lockState = lockState ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !lockState;
        }


    }
}