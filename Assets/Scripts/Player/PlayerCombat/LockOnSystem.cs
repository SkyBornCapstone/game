using UnityEngine;
using PurrNet;
using Unity.Cinemachine;
namespace Player.PlayerCombat
{
    public class LockOnSystem : NetworkBehaviour
    {
        [Header("Lock-On Settings")] [SerializeField]
        private float detectionRadius = 20f;

        // [SerializeField] private float maxLockOnDistnce = 150f;
        [SerializeField] private LayerMask targetLayers;
        [SerializeField] private KeyCode lockOnKey = KeyCode.Q;
        [SerializeField] private Transform playerBody;

        [Header("Camera Smoothing")] [SerializeField]
        private float lockOnRotationSpeed = 5f;

        private LockOnTarget _ownLockOnTarget;
        private bool _isLockedOn;
        private LockOnTarget _lockedTarget;
        
        private Camera _mainCamera;
        private CinemachinePanTilt _panTilt;
        
        private static readonly Collider[] _overlapBuffer = new Collider[32];

        private FirstPersonCamera _firstPersonCamera;
        
        protected override void OnSpawned()
        {
            if (!isOwner)
            {
                enabled = false; 
            }
        }

        private void Start()
        {
            _mainCamera = Camera.main;
            
            var mainCameraComp = InstanceHandler.GetInstance<MainCamera>();
            mainCameraComp.TryGetComponent(out _panTilt);
            
            _firstPersonCamera = GetComponent<FirstPersonCamera>();
            _ownLockOnTarget = GetComponent<LockOnTarget>();
        }

        private void Update()
        {
            if (!isOwner) return;

            if (Input.GetKeyDown(lockOnKey))
            {
                if (_isLockedOn)
                    ClearLockOn();
                else
                    TryLockOn();
            }

            if (_isLockedOn && (_lockedTarget == null || !_lockedTarget.gameObject.activeInHierarchy ||
                                !_lockedTarget.CanBeLocked))
            {
                ClearLockOn();
            }

            if (_isLockedOn)
                TrackLockedTarget();
        }

        private void TryLockOn()
        {
            int count = Physics.OverlapSphereNonAlloc(transform.position, detectionRadius, _overlapBuffer, targetLayers);
            if (count == 0) return;
            
            float xMin = Screen.width  * 0.25f;
            float xMax = Screen.width  * 0.75f;
            float yMin = Screen.height * 0.25f;
            float yMax = Screen.height * 0.75f;

            Vector2 screenCentre = new Vector2(Screen.width / 2f, Screen.height / 2f);
            LockOnTarget bestTarget = null;
            float bestDist = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                if (!_overlapBuffer[i].TryGetComponent(out LockOnTarget target)) continue;
                if (!target.CanBeLocked) continue;

                Vector3 screenPos = _mainCamera.WorldToScreenPoint(target.aimPoint.position);
                if (screenPos.z < 0) continue;
                if (screenPos.x < xMin || screenPos.x > xMax ||
                    screenPos.y < yMin || screenPos.y > yMax) continue;

                float dist = Vector2.Distance(new Vector2(screenPos.x, screenPos.y), screenCentre);
                if (dist < bestDist)
                {
                    bestTarget = target;
                    bestDist = dist;
                }
            }

            if (bestTarget != null && bestTarget != _ownLockOnTarget)
                ApplyLockOn(bestTarget);
        }

        private void ApplyLockOn(LockOnTarget target)
        {
            _lockedTarget = target;
            _isLockedOn = true;
            
            if (_panTilt != null)
            {
                _firstPersonCamera?.SetMouseLookEnabled(false);
            }
        }

        private void ClearLockOn()
        {
            _isLockedOn = false;
            _lockedTarget = null;
            _firstPersonCamera?.SetMouseLookEnabled(true);
        }

        private void TrackLockedTarget()
        {
            Vector3 dirToTarget = (_lockedTarget.aimPoint.position - _mainCamera.transform.position).normalized;
            
            float targetPan = Mathf.Atan2(dirToTarget.x, dirToTarget.z) *  Mathf.Rad2Deg;
            float targetTilt = -Mathf.Asin(dirToTarget.y) *  Mathf.Rad2Deg;
            
            // _panTilt.PanAxis.Value  = Mathf.LerpAngle(_panTilt.PanAxis.Value,  targetPan,  Time.deltaTime * lockOnRotationSpeed);
            // _panTilt.TiltAxis.Value = Mathf.LerpAngle(_panTilt.TiltAxis.Value, targetTilt, Time.deltaTime * lockOnRotationSpeed);
            
            _panTilt.PanAxis.Value  = Mathf.MoveTowardsAngle(_panTilt.PanAxis.Value, targetPan,  lockOnRotationSpeed * Time.deltaTime * 100f);
            _panTilt.TiltAxis.Value = Mathf.MoveTowardsAngle(_panTilt.TiltAxis.Value, targetTilt, lockOnRotationSpeed * Time.deltaTime * 100f);
        }
        
        public bool IsLockedOn => _isLockedOn;
        public LockOnTarget LockedTarget => _lockedTarget;
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = _isLockedOn ? Color.red : Color.cyan;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);

            if (_isLockedOn && _lockedTarget != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, _lockedTarget.aimPoint.position);
            }
        }

    }
}