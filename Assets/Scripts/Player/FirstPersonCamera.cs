using PurrNet;
using UnityEngine;

namespace Player
{
    public class FirstPersonCamera : NetworkBehaviour
    {
        [SerializeField] private float lookSensitivity = 2f;
        [SerializeField] private float maxLookAngle = 80f;
        [SerializeField] private Transform target;

        private Vector2 _currentRotation;
        private bool _initialized;
        private Camera _mainCamera;
        private Rigidbody _rb;

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

        private void RotatePlayerTowardsCamera()
        {
            if (_mainCamera && _rb)
            {
                Vector3 cameraForward = _mainCamera.transform.forward;
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