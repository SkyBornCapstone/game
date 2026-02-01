using PurrNet;
using UnityEngine;

namespace Ship.ShipControllers
{
    public class ThrottleControl : ShipControlStation
    {
        [Header("References")] [SerializeField]
        private ShipControllerV2 ship;

        [SerializeField] private Transform throttleVisual;

        [Header("Settings")] [SerializeField] private float minRotation;
        [SerializeField] private float maxRotation = 270f;
        [SerializeField] private float throttleTurnRate = 90f;

        private readonly SyncVar<float> _currentAngle = new();
        private Quaternion _initialRotation;

        void Awake()
        {
            if (throttleVisual)
                _initialRotation = throttleVisual.localRotation;
        }

        protected override void Update()
        {
            base.Update();
            UpdateVisual();
        }

        protected override void HandleInput()
        {
            float input = Input.GetAxisRaw("Vertical");

            MoveThrottle(input);
        }

        [ServerRpc]
        private void MoveThrottle(float input)
        {
            _currentAngle.value += throttleTurnRate * Time.deltaTime * input;
            _currentAngle.value = Mathf.Clamp(_currentAngle, minRotation, maxRotation);
            ship.SetForwardThrottle(_currentAngle / maxRotation);
        }

        private void UpdateVisual()
        {
            if (throttleVisual)
            {
                Quaternion targetRotation = _initialRotation * Quaternion.Euler(0f, _currentAngle, 0f);

                throttleVisual.localRotation = Quaternion.RotateTowards(
                    throttleVisual.localRotation,
                    targetRotation,
                    throttleTurnRate * Time.deltaTime
                );
            }
        }
    }
}