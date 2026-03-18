using UnityEngine;
using PurrNet;

namespace Player.PlayerCombat
{
    [RequireComponent(typeof(AudioSource))]
    public class ArmAimController : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] public LockOnSystem lockOnSystem;
        [SerializeField] public ArmIKController armIKController;
        [SerializeField] public Animator animator;
        
        [Header("Stance Settings")]
        [SerializeField] private float stanceSwitchThreshold = 1.5f;
        [SerializeField] private float stanceSwitchCooldown = 0.4f;
        
        [Header("Sound Effects")]
        [SerializeField] private AudioClip swingSound;
        private AudioSource _audioSource;
        
        public string _side = "LEFT";
        private float _lastStanceSwitchTime = -999f;
        
        private static readonly int RightSwing  = Animator.StringToHash("RightSwing");
        private static readonly int LeftSwing   = Animator.StringToHash("LeftSwing");
        private static readonly int LeftStance  = Animator.StringToHash("LeftStance");
        private static readonly int RightStance = Animator.StringToHash("RightStance");

        protected override void OnSpawned()
        {
            if (!isOwner) enabled = false;
            _audioSource = GetComponent<AudioSource>();
        }

        private void Update()
        {
            bool isLockedOn = lockOnSystem.IsLockedOn;

            float currentWeight = animator != null ? animator.GetLayerWeight(1) : 0f;
            animator?.SetLayerWeight(1, Mathf.MoveTowards(currentWeight, isLockedOn ? 1f : 0f, Time.deltaTime * 5f));

            if (!isLockedOn) return;

            bool canSwitch = Time.time - _lastStanceSwitchTime >= stanceSwitchCooldown;
            float mouseX   = Input.GetAxis("Mouse X");

            if (canSwitch)
            {
                if (mouseX > stanceSwitchThreshold && _side == "LEFT")
                {
                    print(mouseX + "MOVE RIGHT");
                    _side = "RIGHT";
                    _lastStanceSwitchTime = Time.time;
                    animator?.SetTrigger(RightStance);
                }
                else if (mouseX < -stanceSwitchThreshold && _side == "RIGHT")
                {
                    print(mouseX);
                    _side = "LEFT";
                    _lastStanceSwitchTime = Time.time;
                    animator?.SetTrigger(LeftStance);
                }
            }

            if (Input.GetMouseButtonDown(0))
            {
                animator?.SetTrigger(_side == "RIGHT" ? RightSwing : LeftSwing);
                if (_audioSource != null && swingSound != null)
                {
                    _audioSource.PlayOneShot(swingSound);
                }
            }
        }
    }
}