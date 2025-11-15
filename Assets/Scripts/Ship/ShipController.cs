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

        [Header("Interaction")]
        public float interactDistance = 3f;
        public LayerMask interactLayer = ~0;

        private InputSystem_Actions controls;

        [Header("Debug")]
        public bool debugInput = false;

        private Vector2 moveInput;
        private float upInput;

        private bool isInteracting = false;

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

            // Interact: only grant control if player is properly interacting with the ShipController
            controls.Player.Interact.performed += ctx =>
            {
                var cam = Camera.main;
                if (cam == null)
                {
                    if (debugInput) Debug.LogWarning("[ShipController] No main camera found for Interact check.");
                    return;
                }

                Ray ray = new Ray(cam.transform.position, cam.transform.forward);
                if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayer))
                {
                    // Allow interaction if the ray hit this object or a child collider
                    if (hit.collider != null && (hit.collider.transform == transform || hit.collider.transform.IsChildOf(transform)))
                    {
                        isInteracting = true;
                        if (debugInput) Debug.Log($"[ShipController] Interact started with {name} (within {interactDistance}m)");
                    }
                }
            };
            controls.Player.Interact.canceled += ctx =>
            {
                isInteracting = false;
                if (debugInput) Debug.Log("[ShipController] Interact canceled");
            };
        }

        private void Start()
        {
            ShipEngine[] allEngines = GetComponentsInChildren<ShipEngine>(true);

            // Fallback: if no child engines, search the entire scene
            if ((allEngines == null || allEngines.Length == 0))
            {
                var sceneEngines = UnityEngine.Resources.FindObjectsOfTypeAll<ShipEngine>();
                if (sceneEngines != null && sceneEngines.Length > 0)
                {
                    Debug.LogWarning("[ShipController] No child ShipEngine components found; falling back to scene-wide search.");
                    allEngines = sceneEngines;
                }
            }

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
                string id = (engine.engineID ?? string.Empty).Trim();
                switch (id.ToUpperInvariant())
                {
                    case "L": leftList.Add(engine); break;
                    case "R": rightList.Add(engine); break;
                    case "U": upList.Add(engine); break;
                    default:
                        if (debugInput)
                            Debug.Log($"[ShipController] Unrecognized engineID '{engine.engineID}' on {engine.name}");
                        break;
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
            // Only apply player inputs to the ship if the player is interacting with it
            if (!isInteracting)
            {
                if (debugInput)
                    Debug.Log($"[ShipController] Not interacting; ignoring player input for {name}.");
                return;
            }

            // Treat throttle values as rate inputs in the range -1 to 1.
            // Positive = request increase, Negative = request decrease, Zero = hold.
            float forwardInput = moveInput.y * forwardSensitivity;

            // Turning: positive moveInput.x means turn right -> increase left engines, decrease right engines
            float leftInput  = forwardInput + (moveInput.x * turnSensitivity);
            float rightInput = forwardInput + (-moveInput.x * turnSensitivity);

            leftInput = Mathf.Clamp(leftInput, -1f, 1f);
            rightInput = Mathf.Clamp(rightInput, -1f, 1f);

            SetThrottle(leftEngines, leftInput);
            SetThrottle(rightEngines, rightInput);

            // Vertical thrust (0 - 1 from jump atm, this needs a left shift or something to go down)
            float upInputCmd = upInput * upSensitivity;
            upInputCmd = Mathf.Clamp(upInputCmd, -1f, 1f);
            SetThrottle(upEngines, upInputCmd);

            if (debugInput)
            {
                Debug.Log($"[ShipController] Throttle Inputs -> Left:{leftInput:F2} Right:{rightInput:F2} Up:{upInputCmd:F2} Move:{moveInput} UpInput:{upInput}");
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
