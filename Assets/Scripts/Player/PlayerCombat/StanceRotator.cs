using UnityEngine;

namespace Player.PlayerCombat
{
    public class StanceRotator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ArmAimController armAimController;
        [SerializeField] private LockOnSystem lockOnSystem;

        [Header("Rotation Settings")]
        [SerializeField] private float leftStanceAngle = -15f;
        [SerializeField] private float rightStanceAngle = 15f;
        [SerializeField] private float rotationSpeed = 5f;
        [SerializeField] private Vector3 rotationAxis = Vector3.up;

        private float _currentAngle;

        private void Update()
        {
            bool isLockedOn = lockOnSystem != null && lockOnSystem.IsLockedOn;

            float targetAngle = 0f;

            if (isLockedOn && armAimController != null)
            {
                targetAngle = armAimController._side == "RIGHT" ? rightStanceAngle : leftStanceAngle;
            }

            _currentAngle = Mathf.LerpAngle(_currentAngle, targetAngle, Time.deltaTime * rotationSpeed);
            transform.localRotation = Quaternion.AngleAxis(_currentAngle, rotationAxis);
        }
    }
}