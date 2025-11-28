using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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
        public Transform interactionAnchor;
        public bool parentPlayerToAnchor = true;
        [Header("UI Prompt")]
        public bool enableInteractPrompt = true;
        public string interactPromptText = "Press E to Interact";
        public Vector2 interactPromptScreenOffset = new Vector2(0, -100);

        private Canvas _promptCanvas;
        private Text _promptTextUI;

        private InputSystem_Actions controls;

        [Header("Debug")]
        public bool debugInput = false;

        private Vector2 moveInput;
        private float upInput;

        private bool isInteracting = false;
        private GameObject interactingPlayer = null;
        private Player.PlayerMovement interactingPlayerMovement = null;
        private Transform interactingOriginalParent = null;
        private Vector3 interactingOriginalLocalPos;
        private Quaternion interactingOriginalLocalRot;

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

            controls.Player.Crouch.performed += ctx =>
            {
                upInput = -1f;
                if (debugInput) Debug.Log("[ShipController] Crouch performed -> upInput=-1");
            };
            controls.Player.Crouch.canceled  += ctx =>
            {
                upInput = 0f;
                if (debugInput) Debug.Log("[ShipController] Crouch canceled -> upInput=0");
            };

            // Interact: toggle interaction on press when looking at this ship (press again to release)
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
                        var playerRoot = cam.transform.root.gameObject;

                        if (!isInteracting)
                        {
                            StartInteractionWith(playerRoot, hit);
                        }
                        else
                        {
                            if (interactingPlayer == playerRoot)
                                StopInteraction();
                        }

                        if (debugInput) Debug.Log($"[ShipController] Interact toggled for {name}. isInteracting={isInteracting}");
                    }
                }
            };

            if (enableInteractPrompt)
                EnsurePromptUI();
        }

        private void EnsurePromptUI()
        {
            if (_promptTextUI != null) return;

            // Try to find an existing global prompt by name
            var existing = GameObject.Find("GlobalInteractPrompt");
            if (existing != null)
            {
                _promptTextUI = existing.GetComponentInChildren<Text>();
                _promptCanvas = existing.GetComponentInParent<Canvas>();
                if (_promptTextUI != null)
                    _promptTextUI.enabled = false;
                return;
            }

            // Create a Canvas
            var canvasGO = new GameObject("GlobalInteractPromptCanvas");
            _promptCanvas = canvasGO.AddComponent<Canvas>();
            _promptCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            // Create prompt root
            var promptRoot = new GameObject("GlobalInteractPrompt");
            promptRoot.transform.SetParent(canvasGO.transform, false);

            // Add Text
            var textGO = new GameObject("PromptText");
            textGO.transform.SetParent(promptRoot.transform, false);
            _promptTextUI = textGO.AddComponent<Text>();
            _promptTextUI.alignment = TextAnchor.MiddleCenter;
            _promptTextUI.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _promptTextUI.text = interactPromptText;
            _promptTextUI.fontSize = 24;
            _promptTextUI.color = Color.white;

            // Position text in the center + offset
            var rect = _promptTextUI.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = interactPromptScreenOffset;

            _promptTextUI.enabled = false;
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
            // Update interact prompt visibility for player looking at this ship
            if (enableInteractPrompt && !isInteracting)
            {
                UpdatePromptForCamera(Camera.main);
            }
            else
            {
                if (_promptTextUI != null && _promptTextUI.enabled)
                    _promptTextUI.enabled = false;
            }
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

        private void StartInteractionWith(GameObject playerRoot, RaycastHit hit)
        {
            if (playerRoot == null) return;
            if (isInteracting)
            {
                if (debugInput) Debug.Log("[ShipController] Already interacting; ignoring start request.");
                return;
            }

            interactingPlayer = playerRoot;
            var t = interactingPlayer.transform;
            interactingOriginalParent = t.parent;
            interactingOriginalLocalPos = t.localPosition;
            interactingOriginalLocalRot = t.localRotation;

            // Snap the player to the interaction anchor or the hit point
            if (interactionAnchor != null)
            {
                if (parentPlayerToAnchor)
                {
                    t.SetParent(interactionAnchor, worldPositionStays: false);
                    t.localPosition = Vector3.zero;
                    t.localRotation = Quaternion.identity;
                }
                else
                {
                    t.SetParent(interactingOriginalParent, worldPositionStays: true);
                    t.position = interactionAnchor.position;
                    t.rotation = interactionAnchor.rotation;
                }
            }
            else
            {
                // No anchor provided: snap to the hit point on the ship
                t.position = hit.point;
                t.rotation = transform.rotation;
            }

            // Try to disable player movement so they remain locked (this doesn't work atm)
            interactingPlayerMovement = interactingPlayer.GetComponentInChildren<Player.PlayerMovement>();
            if (interactingPlayerMovement != null)
            {
                interactingPlayerMovement.enabled = false;
            }

            isInteracting = true;
            if (_promptTextUI != null && _promptTextUI.enabled)
                _promptTextUI.enabled = false;

            if (debugInput) Debug.Log($"[ShipController] Player '{interactingPlayer.name}' started interacting with '{name}'");
        }

        private void StopInteraction()
        {
            if (!isInteracting || interactingPlayer == null)
            {
                if (debugInput) Debug.Log("[ShipController] No active interaction to stop.");
                interactingPlayer = null;
                isInteracting = false;
                return;
            }

            var t = interactingPlayer.transform;

            // Restore parent and local transform
            t.SetParent(interactingOriginalParent, worldPositionStays: false);
            t.localPosition = interactingOriginalLocalPos;
            t.localRotation = interactingOriginalLocalRot;

            // Re-enable player movement
            if (interactingPlayerMovement != null)
            {
                interactingPlayerMovement.enabled = true;
                interactingPlayerMovement = null;
            }

            if (debugInput) Debug.Log($"[ShipController] Player '{interactingPlayer.name}' stopped interacting with '{name}'");

            interactingPlayer = null;
            interactingOriginalParent = null;
            isInteracting = false;
            if (_promptTextUI != null && _promptTextUI.enabled)
                _promptTextUI.enabled = false;
        }

        private void UpdatePromptForCamera(Camera cam)
        {
            if (cam == null || _promptTextUI == null) return;

            Ray ray = new Ray(cam.transform.position, cam.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayer))
            {
                if (hit.collider != null && (hit.collider.transform == transform || hit.collider.transform.IsChildOf(transform)))
                {
                    _promptTextUI.text = interactPromptText;
                    _promptTextUI.enabled = true;
                    return;
                }
            }

            _promptTextUI.enabled = false;
        }
    }
}