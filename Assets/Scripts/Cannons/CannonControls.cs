using PurrNet.Prediction;
using UnityEngine;

namespace Player
{
    public class CannonController : PredictedIdentity<CannonController.CannonInput, CannonController.CannonState>
    {
        [Header("Cannon Parts")] 
        [SerializeField] private Transform currentBase;
        [SerializeField] private Transform barrel;
        [SerializeField] private Transform seatPosition;
        
        [Header("Rotation Settings")]
        [SerializeField] private float yawRotationSpeed = 30f;
        [SerializeField] private float pitchRotationSpeed = 20f;
        
        [Header("Rotation Limits")]
        [SerializeField] private float yawLimit = 90f;
        [SerializeField] private float upPitchLimit = 45f;
        [SerializeField] private float downPitchLimit = 10f;
        [SerializeField] private bool invertPitchLimits = false; // Toggle to invert up/down limits
        
        [Header("Base Rotation Offset")]
        [SerializeField] private float barrelBaseRotationX = 90f; // Set this to your barrel's base rotation

        private PlayerMovement currentPlayer;
        private float currentTurretAngle;
        private float currentBarrelAngle;
        
        protected override void LateAwake()
        {
            if (currentBase)
            {
                currentTurretAngle = currentBase.localEulerAngles.y;
            }

            if (barrel)
            {
                // Start from zero, we'll add the offset when applying rotation
                currentBarrelAngle = 0f;
            }
        }
        
        protected override void Simulate(CannonInput input, ref CannonState state, float delta)
        {
            if (!input.isActive)
                return;

            // Rotate turret (left/right) - Yaw
            if (currentBase)
            {
                currentTurretAngle += input.horizontalInput * yawRotationSpeed * delta;
                
                // Clamp yaw within limits
                currentTurretAngle = Mathf.Clamp(currentTurretAngle, -yawLimit, yawLimit);
                
                currentBase.localRotation = Quaternion.Euler(0, currentTurretAngle, 0);
                state.turretAngle = currentTurretAngle;
            }

            // Rotate barrel (up/down) - Pitch
            if (barrel)
            {
                currentBarrelAngle -= input.verticalInput * pitchRotationSpeed * delta;
                
                // Clamp pitch within limits (negative is down, positive is up)
                // If inverted, swap the limits
                float minLimit = invertPitchLimits ? -upPitchLimit : -downPitchLimit;
                float maxLimit = invertPitchLimits ? downPitchLimit : upPitchLimit;
                currentBarrelAngle = Mathf.Clamp(currentBarrelAngle, minLimit, maxLimit);
                
                // Apply rotation WITH the base offset
                barrel.localRotation = Quaternion.Euler(barrelBaseRotationX + currentBarrelAngle, 0, 0);
                state.barrelAngle = currentBarrelAngle;
            }
        }

        protected override void UpdateView(CannonState viewState, CannonState? verified)
        {
            // Sync visual representation with state
            if (currentBase != null)
                currentBase.localRotation = Quaternion.Euler(0, viewState.turretAngle, 0);
            
            if (barrel != null)
                // Apply rotation WITH the base offset
                barrel.localRotation = Quaternion.Euler(barrelBaseRotationX + viewState.barrelAngle, 0, 0);
        }

        protected override void UpdateInput(ref CannonInput input)
        {
            // Additional input processing if needed
        }

        protected override void GetFinalInput(ref CannonInput input)
        {
            if (currentPlayer == null)
            {
                input.isActive = false;
                return;
            }

            input.isActive = true;
            
            // Use WASD for cannon control
            // A/D for left/right turret rotation (Yaw)
            input.horizontalInput = Input.GetAxis("Horizontal");
            // W/S for up/down barrel elevation (Pitch)
            input.verticalInput = Input.GetAxis("Vertical");
        }

        protected override void SanitizeInput(ref CannonInput input)
        {
            // Clamp input values
            input.horizontalInput = Mathf.Clamp(input.horizontalInput, -1f, 1f);
            input.verticalInput = Mathf.Clamp(input.verticalInput, -1f, 1f);
        }

        // Call this when player enters the cannon
        public void EnterCannon(PlayerMovement player)
        {
            currentPlayer = player;
        }

        // Call this when player exits the cannon
        public void ExitCannon()
        {
            currentPlayer = null;
        }

        public Transform GetSeatPosition()
        {
            return seatPosition;
        }

        private void OnDrawGizmosSelected()
        {
            // Visualize yaw limits
            if (currentBase != null)
            {
                Gizmos.color = Color.yellow;
                Vector3 center = currentBase.position;
                
                // Draw yaw rotation arc
                Quaternion minRot = Quaternion.Euler(0, -yawLimit, 0);
                Quaternion maxRot = Quaternion.Euler(0, yawLimit, 0);
                
                Vector3 forward = currentBase.parent ? currentBase.parent.forward : Vector3.forward;
                Gizmos.DrawRay(center, minRot * forward * 2f);
                Gizmos.DrawRay(center, maxRot * forward * 2f);
            }

            // Visualize pitch limits
            if (barrel != null)
            {
                Gizmos.color = Color.cyan;
                Vector3 center = barrel.position;
                
                // Draw barrel elevation limits (accounting for base rotation and inversion)
                Quaternion baseOffset = Quaternion.Euler(barrelBaseRotationX, 0, 0);
                float minAngle = invertPitchLimits ? -upPitchLimit : -downPitchLimit;
                float maxAngle = invertPitchLimits ? downPitchLimit : upPitchLimit;
                Quaternion minRot = Quaternion.Euler(minAngle, 0, 0);
                Quaternion maxRot = Quaternion.Euler(maxAngle, 0, 0);
                
                Vector3 parentForward = barrel.parent ? barrel.parent.forward : Vector3.forward;
                Quaternion parentRot = barrel.parent ? barrel.parent.rotation : Quaternion.identity;
                
                Gizmos.DrawRay(center, parentRot * baseOffset * minRot * Vector3.forward * 2f);
                Gizmos.DrawRay(center, parentRot * baseOffset * maxRot * Vector3.forward * 2f);
            }

            // Draw seat position
            if (seatPosition != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(seatPosition.position, 0.3f);
            }
        }

        public struct CannonState : IPredictedData<CannonState>
        {
            public float turretAngle;
            public float barrelAngle;

            public void Dispose()
            {
            }
        }

        public struct CannonInput : IPredictedData
        {
            public bool isActive;
            public float horizontalInput;
            public float verticalInput;

            public void Dispose()
            {
            }
        }
    }
}