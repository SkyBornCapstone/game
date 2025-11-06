using Unity.Cinemachine;
using UnityEngine;

namespace Player
{
    public class FirstPersonCamera : MonoBehaviour
    {
        [SerializeField] private float lookSensitivity = 2f;
        [SerializeField] private float maxLookAngle = 80f;
        [SerializeField] private CinemachineCamera cinemachineCamera;
    
        private Vector2 _currentRotation;
        private bool _initialized;
    
        public Vector3 forward => Quaternion.Euler(_currentRotation.x, _currentRotation.y, 0) * Vector3.forward;

        private void Awake()
        {
            cinemachineCamera.Priority.Value = -1;
        }

        public void Init()
        {
            _initialized = true;
            cinemachineCamera.Priority.Value = 10;
        }

        private void LateUpdate()
        {
            if (!_initialized) return;
        
            float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;
        
            _currentRotation.x = Mathf.Clamp(_currentRotation.x - mouseY, -maxLookAngle, maxLookAngle);
            _currentRotation.y += mouseX;
        
            transform.localRotation = Quaternion.Euler(_currentRotation.x, 0, 0);
        }
    }
}
