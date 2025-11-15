using UnityEngine;
using UnityEngine.InputSystem;

namespace Ship
{
    [DisallowMultipleComponent]
    public class ShipController : MonoBehaviour
    {
        private ShipEngine[] leftEngines;
        private ShipEngine[] rightEngines;
        private ShipEngine[] upEngines;

        [Header("Input Sensitivity")]
        [Range(0f, 1f)] public float forwardSensitivity = 1f;
        [Range(0f, 1f)] public float turnSensitivity = 1f;
        [Range(0f, 1f)] public float upSensitivity = 1f;

            private InputSystem_Actions controls;

            [Header("Debug")]
            public bool debugInput = false;

        private Vector2 moveInput;
        private float upInput;

        private void Awake()
        {
            controls = new InputSystem_Actions();

            // Read Move vector2 input
            controls.Player.Move.performed += ctx =>
            {
                moveInput = ctx.ReadValue<Vector2>();
                if (debugInput) Debug.Log($"[ShipController] Move performed: {moveInput}");
            };
            controls.Player.Move.canceled  += ctx =>
            {
                moveInput = Vector2.zero;
                if (debugInput) Debug.Log("[ShipController] Move canceled");
            };

            // Optional: use Jump button for upward thrust
            controls.Player.Jump.performed += ctx =>
            {
                upInput = 1f;
                if (debugInput) Debug.Log("[ShipController] Jump performed -> upInput=1");
            };
            controls.Player.Jump.canceled  += ctx =>
            {
                upInput = 0f;
                if (debugInput) Debug.Log("[ShipController] Jump canceled -> upInput=0");
            };
        }

        private void Start()
        {
            ShipEngine[] allEngines = GetComponentsInChildren<ShipEngine>(true);

            if (debugInput)
            {
                if (allEngines == null || allEngines.Length == 0)
                    Debug.LogWarning("[ShipController] No ShipEngine components found in children (includeInactive=true).");
                else
                {
                    Debug.Log($"[ShipController] Found {allEngines.Length} ShipEngine(s):");
                    foreach (var e in allEngines)
                        Debug.Log($"  - {e.name} (engineID='{e.engineID}')");
                }
            }

            var leftList = new System.Collections.Generic.List<ShipEngine>();
            var rightList = new System.Collections.Generic.List<ShipEngine>();
            var upList = new System.Collections.Generic.List<ShipEngine>();

            foreach (var engine in allEngines)
            {
                switch (engine.engineID)
                {
                    case "L": leftList.Add(engine); break;
                    case "R": rightList.Add(engine); break;
                    case "U": upList.Add(engine); break;
                }
            }

            leftEngines = leftList.ToArray();
            rightEngines = rightList.ToArray();
            upEngines = upList.ToArray();

            if (debugInput)
                Debug.Log($"[ShipController] Engine groups -> Left:{leftEngines.Length} Right:{rightEngines.Length} Up:{upEngines.Length}");
        }

        private void OnEnable()
        {
            controls.Enable();
        }

        private void OnDisable()
        {
            controls.Disable();
        }

        private void Update()
        {
            // Forward/backward input = moveInput.y
            float forwardThrottle = Mathf.Clamp01(moveInput.y * forwardSensitivity);

            // Turning left/right = moveInput.x
            float leftTurn  = Mathf.Clamp01(moveInput.x * turnSensitivity);
            float rightTurn = Mathf.Clamp01(-moveInput.x * turnSensitivity);

            float leftThrottle  = forwardThrottle + leftTurn;
            float rightThrottle = forwardThrottle + rightTurn;

            // Ensure throttles remain in the expected 0..1 range
            leftThrottle = Mathf.Clamp01(leftThrottle);
            rightThrottle = Mathf.Clamp01(rightThrottle);

            SetThrottle(leftEngines, leftThrottle);
            SetThrottle(rightEngines, rightThrottle);

            // Vertical thrust
            float upThrottle = Mathf.Clamp01(upInput * upSensitivity);
            SetThrottle(upEngines, upThrottle);

            if (debugInput)
            {
                Debug.Log($"[ShipController] Throttles -> Left:{leftThrottle:F2} Right:{rightThrottle:F2} Up:{upThrottle:F2} Move:{moveInput} UpInput:{upInput}");
            }
        }

        private void SetThrottle(ShipEngine[] engines, float throttle)
        {
            if (engines == null || engines.Length == 0)
            {
                if (debugInput) Debug.Log("[ShipController] SetThrottle called with no engines.");
                return;
            }

            foreach (var engine in engines)
            {
                if (engine != null)
                    engine.throttle = throttle;
                else if (debugInput)
                    Debug.Log("[ShipController] Encountered null engine while setting throttle.");
            }
        }
    }
}
