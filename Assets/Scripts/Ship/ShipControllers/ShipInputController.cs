using System;
using UnityEngine;
using Player;
namespace Ship.ShipControllers
{
    public class ShipInputController : MonoBehaviour
    {
        private PlayerMovement _currentPlayer;
        public ShipControllerV2 shipController;
        private ShipInteractType? _currentInteractType;
        public SteeringWheelViual steeringWheel;
        public ThrottleVIsual throttleVisual;
        private void Update()
        {
            if (!_currentPlayer || !_currentInteractType.HasValue)
                return;
            
            switch (_currentInteractType)
            {
                case ShipInteractType.Pilot:
                    HandleYawInput();
                    break;
                case ShipInteractType.Throttle:
                    HandleForwardInput();
                    break;
                case ShipInteractType.Updown:
                    HandleLiftInput();
                    break;
            }
        }
        
        private void HandleYawInput()
        {
            if (Input.GetKey(KeyCode.E))
            {
                steeringWheel.TurnWheelRight();
            }
            else if (Input.GetKey(KeyCode.Q))
            {
                steeringWheel.TurnWheelLeft();
            }
            UpdateShipYawFromWheel();
        }

        private void UpdateShipYawFromWheel()
        {
            float wheelAngle = -steeringWheel.CurrentAngle;
            if (Mathf.Abs(wheelAngle) <= 15f)
            {
                shipController.SetYawThrottle(0f);
                return;
            }
            float effectiveAngle = wheelAngle - (Mathf.Sign(wheelAngle) * 15f);
            float maxEffectiveAngle = steeringWheel.maxRotation - 15f;
            float throttle = effectiveAngle / maxEffectiveAngle;
            throttle *= .25f;
            shipController.SetYawThrottle(throttle);
        }
        

        private void HandleLiftInput()
        {
            if (Input.GetKey(KeyCode.E))
            {
                shipController.IncreaseLift();
            }
            else if (Input.GetKey(KeyCode.Q))
            {
                shipController.DecreaseLift();
            }
        }

        private void HandleForwardInput()
        {
            if (Input.GetKey(KeyCode.E))
            {
                throttleVisual.IncreaseThrottle();
            }
            else if (Input.GetKey(KeyCode.Q))
            {
                throttleVisual.DecreaseThrottle();
            }
            UpdateShipForwardFromThrottle();
        }

        private void UpdateShipForwardFromThrottle()
        {
            float throttleAngle = throttleVisual.CurrentAngle;
            float throttle = (throttleAngle / throttleVisual.maxRotation);
            shipController.SetForwardThrottle(throttle);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent(out PlayerMovement player))
                return;
            InteractableShipElement interactableShipObject = GetComponent<InteractableShipElement>();
            if (!interactableShipObject) return;
            
            _currentPlayer = player;
            _currentInteractType = interactableShipObject.shipInteractType;
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out PlayerMovement player))
            {
                if (player == _currentPlayer)
                {
                    _currentPlayer = null;
                    _currentInteractType = null;
                }
                
            }
        }
    }
}
