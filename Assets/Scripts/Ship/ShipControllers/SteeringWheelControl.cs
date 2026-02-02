using PurrNet;
using UnityEngine;

namespace Ship.ShipControllers
{
    public class SteeringWheelControl : ShipControlStation
    {
        [Header("References")] [SerializeField]
        private ShipControllerV2 ship;

        [SerializeField] private Transform wheelVisual;

        [Header("Settings")] [SerializeField] private float maxRotation = 360f;
        [SerializeField] private float wheelTurnRate = 90f;
        [SerializeField] private float deadzone = 15f;
        [SerializeField] private float yawSensitivity = 0.25f;

        private readonly SyncVar<float> _currentAngle = new();
        private Quaternion _initialRotation;

        void Awake()
        {
            if (wheelVisual)
                _initialRotation = wheelVisual.localRotation;
        }

        protected override void Update()
        {
            base.Update();
            UpdateVisual();
        }

        protected override void HandleInput()
        {
            float input = Input.GetAxisRaw("Horizontal");

            MoveWheel(input);
        }

        [ServerRpc]
        private void MoveWheel(float input)
        {
            _currentAngle.value += wheelTurnRate * Time.deltaTime * input;
            _currentAngle.value = Mathf.Clamp(_currentAngle, -maxRotation, maxRotation);

            float wheelAngle = _currentAngle.value;

            // Apply deadzone
            if (Mathf.Abs(wheelAngle) <= deadzone)
            {
                ship.SetYawThrottle(0f);
                return;
            }

            // Calculate effective angle beyond deadzone
            float effectiveAngle = wheelAngle - (Mathf.Sign(wheelAngle) * deadzone);
            float maxEffectiveAngle = maxRotation - deadzone;
            float yawThrottle = (effectiveAngle / maxEffectiveAngle) * yawSensitivity;

            ship.SetYawThrottle(yawThrottle);
        }

        private void UpdateVisual()
        {
            if (wheelVisual)
            {
                Quaternion targetRotation = _initialRotation * Quaternion.Euler(-_currentAngle, 0f, 0f);

                wheelVisual.localRotation = Quaternion.RotateTowards(
                    wheelVisual.localRotation,
                    targetRotation,
                    wheelTurnRate * Time.deltaTime
                );
            }
        }
    }
}