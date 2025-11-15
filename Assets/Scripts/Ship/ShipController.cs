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

        private Vector2 moveInput;
        private float upInput;

        private void Awake()
        {
            controls = new InputSystem_Actions();

            // Read Move vector2 input
            controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
            controls.Player.Move.canceled  += ctx => moveInput = Vector2.zero;

            // Optional: use Jump button for upward thrust
            controls.Player.Jump.performed += ctx => upInput = 1f;
            controls.Player.Jump.canceled  += ctx => upInput = 0f;
        }

        private void Start()
        {
            ShipEngine[] allEngines = GetComponentsInChildren<ShipEngine>();

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

            SetThrottle(leftEngines, leftThrottle);
            SetThrottle(rightEngines, rightThrottle);

            // Vertical thrust
            float upThrottle = Mathf.Clamp01(upInput * upSensitivity);
            SetThrottle(upEngines, upThrottle);
        }

        private void SetThrottle(ShipEngine[] engines, float throttle)
        {
            foreach (var engine in engines)
            {
                if (engine != null)
                    engine.throttle = throttle;
            }
        }
    }
}
