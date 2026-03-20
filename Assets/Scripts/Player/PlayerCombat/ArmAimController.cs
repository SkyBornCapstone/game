using System;
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
        private static readonly int DownSwing   = Animator.StringToHash("DownSwing");
        private static readonly int LeftStance  = Animator.StringToHash("LeftStance");
        private static readonly int RightStance = Animator.StringToHash("RightStance");
        private static readonly int DownStance = Animator.StringToHash("DownStance");
        private String prevStance;
        private static readonly int DrawSword = Animator.StringToHash("DrawSword");
        private bool _combatLayerActive = false;
        
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
            if (isLockedOn && !_combatLayerActive)
            {
                _combatLayerActive = true;
                //animator?.SetLayerWeight(1, 1f);
                animator?.SetTrigger(DrawSword);
                print("HERHEHREHRE");
            }
            else if (!isLockedOn && _combatLayerActive)
            {
                _combatLayerActive = false;
                
            }
            

            if (!isLockedOn) return;

            bool canSwitch = Time.time - _lastStanceSwitchTime >= stanceSwitchCooldown;
            float mouseX   = Input.GetAxis("Mouse X");
            float mouseY =  Input.GetAxis("Mouse Y");
            if (canSwitch)
            {
                if ((Math.Abs(mouseY) > Math.Abs(mouseX) && mouseY > stanceSwitchThreshold) && (_side == "RIGHT"|| _side == "LEFT"))
                {
                    print(mouseY);
                    prevStance = _side;
                    _side = "DOWN";
                    _lastStanceSwitchTime = Time.time;
                    animator?.SetTrigger(DownStance);
                }
                else if ((Math.Abs(mouseY) > Math.Abs(mouseX) && mouseY < -stanceSwitchThreshold) && _side == "DOWN")
                {
                    print(mouseY);
                    _side = prevStance;
                    _lastStanceSwitchTime = Time.time;
                    animator?.SetTrigger(_side == "RIGHT" ? RightStance : LeftStance);
                }
                else if (mouseX > stanceSwitchThreshold && (_side == "LEFT" || _side == "DOWN") )
                {
                    print(mouseX + "MOVE RIGHT");
                    prevStance = _side;
                    _side = "RIGHT";
                    _lastStanceSwitchTime = Time.time;
                    animator?.SetTrigger(RightStance);
                }
                else if (mouseX < -stanceSwitchThreshold && (_side == "RIGHT"|| _side == "DOWN"))
                {
                    print(mouseX);
                    prevStance = _side;
                    _side = "LEFT";
                    _lastStanceSwitchTime = Time.time;
                    animator?.SetTrigger(LeftStance);
                }
                
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (_side == "RIGHT")
                {
                    animator?.SetTrigger(RightSwing);
                }
                else if (_side == "LEFT")
                {
                    animator?.SetTrigger(LeftSwing);
                }
                else if (_side == "DOWN")
                {
                    animator?.SetTrigger(DownSwing);
                }
                //animator?.SetTrigger(_side == "RIGHT" ? RightSwing : LeftSwing);
                if (_audioSource != null && swingSound != null)
                {
                    _audioSource.PlayOneShot(swingSound);
                }
            }
        }
    }
}