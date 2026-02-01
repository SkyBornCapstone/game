using PurrNet;
using UnityEngine;

namespace Player
{
    public class FirstPersonCamera : NetworkBehaviour
    {
        [SerializeField] private Transform target;

        private Camera _mainCamera;
        private Rigidbody _rb;

        private Transform _currentShipVisuals;
        private Transform _currentShipProxy;

        protected override void OnSpawned()
        {
            if (!isOwner)
            {
                enabled = false;
                return;
            }

            InstanceHandler.GetInstance<MainCamera>().SetTarget(target);
        }

        private void Start()
        {
            _mainCamera = Camera.main;
            _rb = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            RotatePlayerTowardsCamera();
        }

        public void SetShipContext(Transform visualShipRoot, Transform proxyShipRoot)
        {
            _currentShipVisuals = visualShipRoot;
            _currentShipProxy = proxyShipRoot;
        }

        public void ClearShipContext()
        {
            _currentShipVisuals = null;
            _currentShipProxy = null;
        }

        private void RotatePlayerTowardsCamera()
        {
            if (!_mainCamera || !_rb) return;

            Vector3 cameraForward = _mainCamera.transform.forward;

            if (_currentShipVisuals && _currentShipProxy)
            {
                Vector3 localDir = Quaternion.Inverse(_currentShipVisuals.rotation) * cameraForward;

                localDir.y = 0;
                localDir.Normalize();

                if (localDir != Vector3.zero)
                {
                    Quaternion targetRot = _currentShipProxy.rotation * Quaternion.LookRotation(localDir);
                    _rb.MoveRotation(targetRot);
                }
            }
            else
            {
                cameraForward.y = 0f;
                if (cameraForward != Vector3.zero)
                {
                    Quaternion newRotation = Quaternion.LookRotation(cameraForward);
                    _rb.MoveRotation(newRotation);
                }
            }
        }
    }
}