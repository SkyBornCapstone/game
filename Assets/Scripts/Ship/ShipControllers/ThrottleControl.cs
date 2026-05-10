using PurrNet;
using UnityEngine;

namespace Ship.ShipControllers
{
    public class ThrottleControl : ShipControlStation
    {
        [Header("References")] [SerializeField]
        private ShipControllerV2 ship;

        [SerializeField] private Transform throttleVisual;
        [SerializeField] private Transform throttleStart;
        [SerializeField] private Transform throttleEnd;
        
        [Header("Settings")] [SerializeField] private float minRotation;
        [SerializeField] private float maxRotation = 270f;
        [SerializeField] private float throttleTurnRate = 90f;
        [SerializeField] private float throttleMoveRate = 1f;
        // private readonly SyncVar<float> _currentAngle = new(ownerAuth: true);
        // private Quaternion _initialRotation;
        private readonly SyncVar<float> _throttleT = new(ownerAuth: true);

        // void Awake()
        // {
        //     // if (throttleVisual)
        //     //     _initialRotation = throttleVisual.localRotation;
        // }

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

        private void MoveThrottle(float input)
        {
            
            // _currentAngle.value += throttleTurnRate * Time.deltaTime * input;
            // _currentAngle.value = Mathf.Clamp(_currentAngle, minRotation, maxRotation);
            // ship.SetForwardThrottle(_currentAngle / maxRotation);
            _throttleT.value = Mathf.Clamp01(_throttleT.value + throttleMoveRate * Time.deltaTime * input);
            ship.SetForwardThrottle(_throttleT.value);
        }

        private void UpdateVisual()
        {
            if (!throttleVisual || !throttleStart || !throttleEnd) return;

            float t = _throttleT.value;

            throttleVisual.position = Vector3.Lerp(throttleStart.position, throttleEnd.position, t);
            throttleVisual.rotation = Quaternion.Lerp(throttleStart.rotation, throttleEnd.rotation, t);
        
            // if (throttleVisual)
            // {
            //     Quaternion targetRotation = _initialRotation * Quaternion.Euler(0f, _currentAngle, 0f);
            //
            //     throttleVisual.localRotation = Quaternion.RotateTowards(
            //         throttleVisual.localRotation,
            //         targetRotation,
            //         throttleTurnRate * Time.deltaTime
            //     );
            // }
        }
    }
}