using UnityEngine;

namespace Ship
{
    [DisallowMultipleComponent]
    public class ShipController : MonoBehaviour
    {
        private ShipEngine[] leftEngines;
        private ShipEngine[] rightEngines;
        private ShipEngine[] upEngines;

        [Header("Input Settings")]
        [Range(0f, 1f)] public float forwardSensitivity = 1f;
        [Range(0f, 1f)] public float turnSensitivity = 1f;
        [Range(0f, 1f)] public float upSensitivity = 1f;

        // Find and categorize all ship engines
        void Start()
        {
            ShipEngine[] allEngines = GetComponentsInChildren<ShipEngine>();

            var leftList = new System.Collections.Generic.List<ShipEngine>();
            var rightList = new System.Collections.Generic.List<ShipEngine>();
            var upList = new System.Collections.Generic.List<ShipEngine>();

            foreach (var engine in allEngines)
            {
                switch (engine.engineID)
                {
                    case "L":
                        leftList.Add(engine);
                        break;
                    case "R":
                        rightList.Add(engine);
                        break;
                    case "U":
                        upList.Add(engine);
                        break;
                    default:
                        // This shouldn't happen
                        break;
                }
            }

            leftEngines = leftList.ToArray();
            rightEngines = rightList.ToArray();
            upEngines = upList.ToArray();
        }

        void Update()
        {
            // Read user inputs, idk if this works yet
            float forwardInput = Input.GetAxis("Vertical");
            float turnInput = Input.GetAxis("Horizontal");
            float upInput = Input.GetAxis("UpDown");

            // Modulate both left and right engines at the same rate to speed up/slow down
            float forwardThrottle = Mathf.Clamp01(forwardInput * forwardSensitivity);

            // Turning by adjusting left/right engines
            float leftThrottle = Mathf.Clamp01(turnInput * turnSensitivity);
            float rightThrottle = Mathf.Clamp01(-turnInput * turnSensitivity);

            // Combine forward throttle with turning throttle
            leftThrottle += forwardThrottle;
            rightThrottle += forwardThrottle;
            SetThrottle(leftEngines, leftThrottle);
            SetThrottle(rightEngines, rightThrottle);

            // Modulate up thrusters to move up/down
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