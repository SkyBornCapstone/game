using UnityEngine;

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

        private ShipControls controls;

        private float forwardInput;
        private float turnInput;
        private float upInput;

        private void Awake()
        {
            // Set up the InputActions instance
            controls = new ShipControls();

            // Register callbacks
            controls.Flight.Forward.performed += ctx => forwardInput = ctx.ReadValue<float>();
            controls.Flight.Forward.canceled  += ctx => forwardInput = 0f;

            controls.Flight.Turn.performed += ctx => turnInput = ctx.ReadValue<float>();
            controls.Flight.Turn.canceled  += ctx => turnInput = 0f;

            controls.Flight.Lift.performed += ctx => upInput = ctx.ReadValue<float>();
            controls.Flight.Lift.canceled  += ctx => upInput = 0f;
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
            // Forward throttle (affects both sides)
            float forwardThrottle = Mathf.Clamp01(forwardInput * forwardSensitivity);

            // Turning (opposes sides)
            float leftTurn = Mathf.Clamp01(turnInput * turnSensitivity);
            float rightTurn = Mathf.Clamp01(-turnInput * turnSensitivity);

            // Combined engine power
            float leftThrottle = forwardThrottle + leftTurn;
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
