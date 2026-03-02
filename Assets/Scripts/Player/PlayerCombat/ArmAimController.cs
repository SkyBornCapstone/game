using UnityEngine;
using PurrNet;
namespace Player.PlayerCombat
{
    public class ArmAimController : NetworkBehaviour
    {
        [Header("References")] [SerializeField]
        public LockOnSystem lockOnSystem;
        [SerializeField] public ArmIKController armIKController;
        [SerializeField] public Transform rightHandTarget;
        [SerializeField] public Transform armAimOrigin;

        [Header("Settings")] [SerializeField] public float mouseSensitivity = .002f;
        [SerializeField] public float armExtendDistance = 1.5f;
        [SerializeField] public Vector2 yawClamp = new Vector2(-60f, 60f);
        [SerializeField] public Vector2 pitchClamp = new Vector2(-60f, 60f);

        private Vector3 _originalPosition;
        private Quaternion _originalRotation;
    
        private float _yaw;
        private float _pitch;
        private bool _wasLockedOn;


        protected override void OnSpawned()
        {
            if (!isOwner) enabled = false;
            _originalPosition = rightHandTarget.localPosition;
            _originalRotation = rightHandTarget.localRotation;
        }

        private void Update()
        {
            bool isLockedOn = lockOnSystem.IsLockedOn;

            if (isLockedOn && !_wasLockedOn)
            {
                _yaw = 0f;
                _pitch = 0f;
            }

            _wasLockedOn = isLockedOn;

            if (!isLockedOn)
            {
                armIKController.rightHandTargetSwing.value = null;
                return;
            }

            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            
            _yaw = Mathf.Clamp(_yaw + mouseX  * mouseSensitivity * 1000f, yawClamp.x, yawClamp.y);
            _pitch = Mathf.Clamp(_pitch - mouseY *  mouseSensitivity * 1000f, pitchClamp.x, pitchClamp.y);
            
            Quaternion aimRotation = armAimOrigin.rotation *  Quaternion.Euler(_pitch, _yaw, 0f);
            rightHandTarget.position = armAimOrigin.position + aimRotation * Vector3.forward * armExtendDistance;
            rightHandTarget.rotation = aimRotation;

            armIKController.rightHandTargetSwing.value = rightHandTarget;
        }
        
        
    }
}

