using PurrNet.Prediction;
using UnityEngine;
using Player;
namespace Cannons
{
    public class CannonController : PredictedIdentity<CannonController.CannonInput, CannonController.CannonState>
    {
        [Header("Cannon Parts")] 
        [SerializeField] private Transform currentBase;
        [SerializeField] private Transform barrel;
        [SerializeField] private Transform seatPosition;
        [SerializeField] private Transform projectileSpawn;
        
        [Header("Rotation Settings")]
        [SerializeField] private float yawRotationSpeed = 30f;
        [SerializeField] private float pitchRotationSpeed = 20f;
        
        [Header("Rotation Limits")]
        [SerializeField] private float yawLimit = 90f;
        [SerializeField] private float upPitchLimit = 45f;
        [SerializeField] private float downPitchLimit = 10f;
        [SerializeField] private bool invertPitchLimits = false; // Toggle to invert up/down limits
        
        [Header("Base Rotation Offset")]
        [SerializeField] private float barrelBaseRotationZ = 90f;

        public Transform Barrel
        {
            get => barrel;
            set => barrel = value;
        }
        [Header("Cannon Ball")] 
        [SerializeField] private float shootForce = 10;
        [SerializeField] private float reloadTime = 3f;
        [SerializeField] private float shootTime = 3f;
        [SerializeField] private GameObject projectilePrefab;

        private PlayerMovement currentPlayer;

        protected override CannonState GetInitialState()
        {
            // Get the actual starting barrel angle by removing the base offset
            float actualBarrelAngle = barrel ? barrel.localEulerAngles.z : 0f;
    
            // Normalize the angle to the -180 to 180 range
            if (actualBarrelAngle > 180f)
                actualBarrelAngle -= 360f;
    
            // Subtract the base offset to get the "game" angle
            float initialBarrelAngle = actualBarrelAngle - barrelBaseRotationZ;
    
            return new CannonState
            {
                turretAngle = currentBase ? currentBase.localEulerAngles.y : 0f,
                barrelAngle = initialBarrelAngle,
                canShoot = true,
                timeToCanShoot = 0f
            };
        }
        
        protected override void Simulate(CannonInput input, ref CannonState state, float delta)
        {
            if (!input.isActive)
                return;

            // Rotate turret (left/right) - Yaw
            if (currentBase)
            {
                state.turretAngle += input.horizontalInput * yawRotationSpeed * delta;
                
                // Clamp yaw within limits
                state.turretAngle = Mathf.Clamp(state.turretAngle, -yawLimit, yawLimit);
                
                // currentBase.localRotation = Quaternion.Euler(0, currentTurretAngle, 0);
            }

            // Rotate barrel (up/down) - Pitch
            if (barrel)
            {
                state.barrelAngle -= input.verticalInput * pitchRotationSpeed * delta;
                
                // Clamp pitch within limits (negative is down, positive is up)
                // If inverted, swap the limits
                float minLimit = invertPitchLimits ? -upPitchLimit : -downPitchLimit;
                float maxLimit = invertPitchLimits ? downPitchLimit : upPitchLimit;
                state.barrelAngle = Mathf.Clamp(state.barrelAngle, minLimit, maxLimit);
                
                // Apply rotation WITH the base offset
                // barrel.localRotation = Quaternion.Euler(barrelBaseRotationX + currentBarrelAngle, 0, 0);
            }

            if (state.canShoot && input.shoot)
            {
                state.timeToCanShoot = shootTime;
                // state.ammo--;


                Vector3 shootDirection = projectileSpawn.right;
                Vector3 spawnPosition = projectileSpawn.position;
                var createdObject = predictionManager.hierarchy.Create(projectilePrefab, spawnPosition, Quaternion.identity);
                if (!createdObject.HasValue)
                    return;
                
                createdObject.Value.TryGetComponent(predictionManager, out PredictedRigidbody rb);
                rb.AddForce(shootDirection * shootForce, ForceMode.Impulse);
            }
            if (!state.canShoot)
                state.timeToCanShoot -= delta;
        }
        protected override void UpdateInput(ref CannonInput input)
        {
            input.shoot |= UnityEngine.Input.GetKeyDown(KeyCode.Mouse0);
        }
        protected override void UpdateView(CannonState viewState, CannonState? verified)
        {
            // Sync visual representation with state
            if (currentBase)
                currentBase.localRotation = Quaternion.Euler(0, viewState.turretAngle, 0);
            
            if (barrel)
                // Apply rotation WITH the base offset
                barrel.localRotation = Quaternion.Euler(0, 0, barrelBaseRotationZ + viewState.barrelAngle);
        }

        protected override void GetFinalInput(ref CannonInput input)
        {
            if (!currentPlayer)
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
                
                Vector3 forward = currentBase.parent ? currentBase.parent.right : Vector3.right;
                Gizmos.DrawRay(center, minRot * forward * 2f);
                Gizmos.DrawRay(center, maxRot * forward * 2f);
            }

            // Visualize pitch limits
            if (barrel != null)
            {
                Gizmos.color = Color.cyan;
                Vector3 center = barrel.position;
                
                // Draw barrel elevation limits (accounting for base rotation and inversion)
                Quaternion baseOffset = Quaternion.Euler(0, 0, barrelBaseRotationZ); 
                float minAngle = invertPitchLimits ? -upPitchLimit : -downPitchLimit;
                float maxAngle = invertPitchLimits ? downPitchLimit : upPitchLimit;
                Quaternion minRot = Quaternion.Euler(minAngle, 0, 0);
                Quaternion maxRot = Quaternion.Euler(maxAngle, 0, 0);
                
                Vector3 parentForward = barrel.parent ? barrel.parent.right : Vector3.right;
                Quaternion parentRot = barrel.parent ? barrel.parent.rotation : Quaternion.identity;
                
                Gizmos.DrawRay(center, parentRot * baseOffset * minRot * Vector3.right * 2f);
                Gizmos.DrawRay(center, parentRot * baseOffset * maxRot * Vector3.right * 2f);
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
            public bool canShoot;
            public float timeToCanShoot;

            public void Dispose()
            {
            }
        }

        public struct CannonInput : IPredictedData
        {
            public bool isActive;
            public float horizontalInput;
            public float verticalInput;
            public bool shoot;

            public void Dispose()
            {
            }
        }
    }
}