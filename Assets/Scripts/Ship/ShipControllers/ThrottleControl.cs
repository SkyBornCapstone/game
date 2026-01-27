using Player;
using UnityEngine;

namespace Ship.ShipControllers
{
    public class ThrottleControl : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ShipControllerV2 ship;
        [SerializeField] private Transform throttleVisual;
        
        [Header("Settings")]
        [SerializeField] private float maxRotation = 270f;
        [SerializeField] private float throttleTurnRate = 90f;
        
        public float CurrentAngle => currentAngle;
        
        private float currentAngle;
        private Quaternion initialRotation;
        private PlayerMovement currentPlayer;
        private const float minRotation = 0f;
        
        void Awake()
        {
            if (throttleVisual)
                initialRotation = throttleVisual.localRotation;
        }

        void Update()
        {
            if (!currentPlayer)
                return;
                
            HandleInput();
            UpdateVisual();
        }

        private void HandleInput()
        {
            float input = ShipInputManager.Instance.GetControlInput();
            
            if (input > 0)
                IncreaseThrottle();
            else if (input < 0)
                DecreaseThrottle();
        }

        private void IncreaseThrottle()
        {
            currentAngle += throttleTurnRate * Time.deltaTime;
            currentAngle = Mathf.Clamp(currentAngle, minRotation, maxRotation);
            UpdateShip();
        }

        private void DecreaseThrottle()
        {
            currentAngle -= throttleTurnRate * Time.deltaTime;
            currentAngle = Mathf.Clamp(currentAngle, minRotation, maxRotation);
            UpdateShip();
        }

        private void UpdateShip()
        {
            if (!ship)
                return;
                
            float throttle = currentAngle / maxRotation;
            ship.SetForwardThrottle(throttle);
        }

        private void UpdateVisual()
        {
            if (throttleVisual)
                throttleVisual.localRotation = initialRotation * Quaternion.Euler(0f, currentAngle, 0f);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out PlayerMovement player))
                currentPlayer = player;
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out PlayerMovement player) && player == currentPlayer)
                currentPlayer = null;
        }
    }
}