using System.Collections.Generic;
using Player;
using UnityEngine;
using UnityEngine.UI;

namespace Ship
{
    [DisallowMultipleComponent]
    public class ShipController : MonoBehaviour
    {
        private ShipEngine[] leftEngines;
        private ShipEngine[] rightEngines;
        private ShipEngine[] upEngines;

        [Header("Input Sensitivity")] [Range(0f, 1f)]
        public float forwardSensitivity = 1f;

        [Range(0f, 1f)] public float turnSensitivity = 1f;
        [Range(0f, 1f)] public float upSensitivity = 1f;

        [Header("Interaction")] public float interactDistance = 3f;
        public LayerMask interactLayer = ~0;
        public Transform interactionAnchor;
        public bool parentPlayerToAnchor = true;
        [Header("UI Prompt")] public bool enableInteractPrompt = true;
        public string interactPromptText = "Press E to Interact";
        public Vector2 interactPromptScreenOffset = new Vector2(0, -100);

        private Canvas _promptCanvas;
        private Text _promptTextUI;

        private InputSystem_Actions controls;

        private Vector2 moveInput;
        private float upInput;

        private bool isInteracting = false;
        private GameObject interactingPlayer = null;

        private List<PlayerMovement> interactingPlayerMovements = new List<PlayerMovement>();

        // Interaction state
        private Transform interactingOriginalParent = null;
        private Vector3 interactingOriginalLocalPos;
        private Quaternion interactingOriginalLocalRot;

        private void Awake()
        {
            controls = new InputSystem_Actions();

            // Read Move vector2 input
            controls.Player.Move.performed += ctx => { moveInput = ctx.ReadValue<Vector2>(); };
            controls.Player.Move.canceled += ctx => { moveInput = Vector2.zero; };

            controls.Player.Jump.performed += ctx => { upInput = 1f; };
            controls.Player.Jump.canceled += ctx => { upInput = 0f; };

            controls.Player.Crouch.performed += ctx => { upInput = -1f; };
            controls.Player.Crouch.canceled += ctx => { upInput = 0f; };

            // Interact: toggle interaction on press when looking at this ship (press again to release)
            controls.Player.Interact.performed += ctx =>
            {
                var cam = Camera.main;
                if (cam == null)
                {
                    return;
                }

                Ray ray = new Ray(cam.transform.position, cam.transform.forward);
                if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayer))
                {
                    // Allow interaction if the ray hit this object or a child collider
                    if (hit.collider != null && (hit.collider.transform == transform ||
                                                 hit.collider.transform.IsChildOf(transform)))
                    {
                        GameObject playerRoot = null;
                        try
                        {
                            var movesAll = FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None);
                            foreach (var m in movesAll)
                            {
                                if (m.isOwner)
                                {
                                    playerRoot = m.gameObject;
                                    break;
                                }
                            }
                        }
                        catch
                        {
                            playerRoot = null;
                        }

                        // Fallback to camera root
                        if (playerRoot == null)
                            playerRoot = cam.transform.root.gameObject;

                        if (!isInteracting)
                            StartInteractionWith(playerRoot, hit);
                        else if (interactingPlayer == playerRoot)
                            StopInteraction();
                    }
                }
            };
        }

        private void Start()
        {
            ShipEngine[] allEngines = GetComponentsInChildren<ShipEngine>(true);

            if (allEngines == null || allEngines.Length == 0)
            {
                var sceneEngines = Resources.FindObjectsOfTypeAll<ShipEngine>();
                if (sceneEngines != null && sceneEngines.Length > 0)
                {
                    allEngines = sceneEngines;
                }
            }

            var leftList = new List<ShipEngine>();
            var rightList = new List<ShipEngine>();
            var upList = new List<ShipEngine>();

            foreach (var engine in allEngines)
            {
                string id = (engine.engineID ?? string.Empty).Trim();
                switch (id.ToUpperInvariant())
                {
                    case "L": leftList.Add(engine); break;
                    case "R": rightList.Add(engine); break;
                    case "U": upList.Add(engine); break;
                    default:
                        break;
                }
            }

            leftEngines = leftList.ToArray();
            rightEngines = rightList.ToArray();
            upEngines = upList.ToArray();
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
            if (!isInteracting) return;

            // Treat throttle values as rate inputs in the range -1 to 1.
            // Positive = request increase, Negative = request decrease, Zero = hold.
            float forwardInput = moveInput.y * forwardSensitivity;

            // Turning: positive moveInput.x means turn right -> increase left engines, decrease right engines
            float leftInput = forwardInput + (moveInput.x * turnSensitivity);
            float rightInput = forwardInput + (-moveInput.x * turnSensitivity);

            leftInput = Mathf.Clamp(leftInput, -1f, 1f);
            rightInput = Mathf.Clamp(rightInput, -1f, 1f);

            SetThrottle(leftEngines, leftInput);
            SetThrottle(rightEngines, rightInput);

            // Vertical thrust (0 - 1 from jump atm, this needs a left shift or something to go down)
            float upInputCmd = upInput * upSensitivity;
            upInputCmd = Mathf.Clamp(upInputCmd, -1f, 1f);
            SetThrottle(upEngines, upInputCmd);
        }

        private void SetThrottle(ShipEngine[] engines, float throttle)
        {
            if (engines == null || engines.Length == 0)
            {
                return;
            }

            foreach (var engine in engines)
            {
                if (engine != null)
                    engine.throttle = throttle;
            }
        }

        private void StartInteractionWith(GameObject playerRoot, RaycastHit hit)
        {
            if (playerRoot == null || isInteracting) return;

            interactingPlayer = playerRoot;
            var t = interactingPlayer.transform;
            interactingOriginalParent = t.parent;
            interactingOriginalLocalPos = t.localPosition;
            interactingOriginalLocalRot = t.localRotation;

            // Now snap the player to the interaction anchor or the hit point (after disabling prediction)
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
                // No anchor provided: snap to the hit point on the ship *this is bad*
                t.position = hit.point;
                t.rotation = transform.rotation;
            }

            // Disable player movement and physics so they remain locked to the ship
            interactingPlayerMovements.Clear();
            var moves = interactingPlayer.GetComponentsInChildren<PlayerMovement>(true);
            foreach (var mv in moves)
            {
                try
                {
                    // Call EnterShip to lock the player to the ship anchor
                    Transform anchor = (interactionAnchor != null) ? interactionAnchor : t;
                    // mv.EnterShip(anchor);
                    interactingPlayerMovements.Add(mv);
                }
                catch
                {
                }
            }

            isInteracting = true;
            if (_promptTextUI != null && _promptTextUI.enabled)
                _promptTextUI.enabled = false;
        }

        private void StopInteraction()
        {
            if (!isInteracting || interactingPlayer == null)
            {
                interactingPlayer = null;
                isInteracting = false;
                return;
            }

            var t = interactingPlayer.transform;

            // Restore parent and local transform
            t.SetParent(interactingOriginalParent, worldPositionStays: false);
            t.localPosition = interactingOriginalLocalPos;
            t.localRotation = interactingOriginalLocalRot;

            // Notify PlayerMovement instances to exit ship mode
            foreach (var mv in interactingPlayerMovements)
            {
                // try { mv.ExitShip(); } catch { }
            }

            interactingPlayerMovements.Clear();

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
                if (hit.collider != null &&
                    (hit.collider.transform == transform || hit.collider.transform.IsChildOf(transform)))
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