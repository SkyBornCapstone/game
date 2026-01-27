using Player;
using PurrNet;
using UnityEngine;

namespace Cannons
{
    public class CannonController : NetworkBehaviour
    {
        [Header("Cannon Parts")] [SerializeField]
        private Transform currentBase;

        [SerializeField] private Transform barrel;
        [SerializeField] private Transform seatPosition;
        [SerializeField] private Transform projectileSpawn;

        [Header("Rotation Settings")] [SerializeField]
        private float yawRotationSpeed = 30f;

        [SerializeField] private float pitchRotationSpeed = 20f;

        [Header("Rotation Limits")] [SerializeField]
        private float yawLimit = 90f;

        [SerializeField] private float upPitchLimit = 45f;
        [SerializeField] private float downPitchLimit = 10f;

        [Header("Cannon Ball")] [SerializeField]
        private float shootForce = 10;

        [SerializeField] private float shootTime = 3f;
        [SerializeField] private GameObject projectilePrefab;

        private PlayerMovement _currentPlayer;
        private float _turretAngle;
        private float _barrelAngle;
        private float _timeToCanShoot;

        protected override void OnSpawned()
        {
            float actualBarrelAngle = barrel ? barrel.localEulerAngles.z : 0f;

            // Normalize the angle to the -180 to 180 range
            if (actualBarrelAngle > 180f)
                actualBarrelAngle -= 360f;

            _turretAngle = currentBase ? currentBase.localEulerAngles.y : 0f;
            _barrelAngle = actualBarrelAngle;
            _timeToCanShoot = 0;
        }

        private void Update()
        {
            if (!_currentPlayer)
                return;

            // Rotate turret (left/right) - Yaw
            var horizontalInput = Input.GetAxis("Horizontal");
            if (currentBase)
            {
                _turretAngle += horizontalInput * yawRotationSpeed * Time.deltaTime;

                // Clamp yaw within limits
                _turretAngle = Mathf.Clamp(_turretAngle, -yawLimit, yawLimit);

                currentBase.localRotation = Quaternion.Euler(0, _turretAngle, 0);
            }

            // Rotate barrel (up/down) - Pitch
            var verticalInput = -Input.GetAxis("Vertical");
            if (barrel)
            {
                _barrelAngle -= verticalInput * pitchRotationSpeed * Time.deltaTime;

                // Clamp pitch within limits
                _barrelAngle = Mathf.Clamp(_barrelAngle, downPitchLimit, upPitchLimit);

                // Apply rotation WITH the base offset
                barrel.localRotation = Quaternion.Euler(0, 0, _barrelAngle);
            }

            if (_timeToCanShoot <= 0 && Input.GetKeyDown(KeyCode.Space))
            {
                _timeToCanShoot = shootTime;

                Vector3 shootDirection = projectileSpawn.right;
                Vector3 spawnPosition = projectileSpawn.position;
                var createdObject = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);

                createdObject.TryGetComponent(out Rigidbody rb);
                rb.AddForce(shootDirection * shootForce, ForceMode.Impulse);
            }

            if (_timeToCanShoot > 0)
                _timeToCanShoot -= Time.deltaTime;
        }

        // Call this when player enters the cannon
        public void EnterCannon(PlayerMovement player)
        {
            _currentPlayer = player;
        }

        // Call this when player exits the cannon
        public void ExitCannon()
        {
            _currentPlayer = null;
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

                Quaternion minRot = Quaternion.Euler(downPitchLimit, 0, 0);
                Quaternion maxRot = Quaternion.Euler(upPitchLimit, 0, 0);

                Quaternion parentRot = barrel.parent ? barrel.parent.rotation : Quaternion.identity;

                Gizmos.DrawRay(center, parentRot * minRot * Vector3.right * 2f);
                Gizmos.DrawRay(center, parentRot * maxRot * Vector3.right * 2f);
            }

            // Draw seat position
            if (seatPosition != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(seatPosition.position, 0.3f);
            }
        }
    }
}