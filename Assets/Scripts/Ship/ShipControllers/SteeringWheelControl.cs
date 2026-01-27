using Player;
using UnityEngine;

namespace Ship.ShipControllers
{
    public class SteeringWheelControl : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ShipControllerV2 ship;
        [SerializeField] private Transform wheelVisual;
        
        [Header("Settings")]
        [SerializeField] private float maxRotation = 360f;
        [SerializeField] private float wheelTurnRate = 90f;
        [SerializeField] private float deadzone = 15f;
        [SerializeField] private float yawSensitivity = 0.25f;
        
        public float CurrentAngle => currentAngle;
        
        private float currentAngle;
        private Quaternion initialRotation;
        private PlayerMovement currentPlayer;

        void Awake()
        {
            if (wheelVisual)
                initialRotation = wheelVisual.localRotation;
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
                TurnWheelRight();
            else if (input < 0)
                TurnWheelLeft();
        }
        
        private void TurnWheelRight()
        {
            currentAngle -= wheelTurnRate * Time.deltaTime;
            currentAngle = Mathf.Clamp(currentAngle, -maxRotation, maxRotation);
            UpdateShip();
        }
        
        private void TurnWheelLeft()
        {
            currentAngle += wheelTurnRate * Time.deltaTime;
            currentAngle = Mathf.Clamp(currentAngle, -maxRotation, maxRotation);
            UpdateShip();
        }

        private void UpdateShip()
        {
            if (!ship)
                return;
                
            float wheelAngle = -currentAngle;
            
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
                wheelVisual.localRotation = initialRotation * Quaternion.Euler(currentAngle, 0f, 0f);
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