using System;
using UnityEngine;
using PurrNet;

namespace Player.PlayerCombat
{
    public class CombatControllerv2 : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] public LockOnSystem lockOnSystem;
        [SerializeField] public ArmIKController armIKController;
        [SerializeField] public NetworkAnimator animator;

        [Header("Stance Settings")]
        // [SerializeField] private float stanceSwitchThreshold = 1.5f;
        // [SerializeField] private float stanceSwitchCooldown = 0.4f;
        [SerializeField] private float blockBreakStunDuration = 1.5f;

        [SerializeField] public string _side = "RIGHT";

        private static readonly int RightSwing  = Animator.StringToHash("RightSwing");
        private static readonly int LeftSwing   = Animator.StringToHash("LeftSwing");
        private static readonly int DownSwing   = Animator.StringToHash("DownSwing");
        private static readonly int LeftStance  = Animator.StringToHash("LeftStance");
        private static readonly int RightStance = Animator.StringToHash("RightStance");
        private static readonly int DownStance  = Animator.StringToHash("DownStance");
        private static readonly int DrawSword   = Animator.StringToHash("DrawSword");
        private static readonly int SheathSword = Animator.StringToHash("SheathSword");
        private static readonly int punch       = Animator.StringToHash("Punch");
        private static readonly int block       = Animator.StringToHash("Block");
        private static readonly int leaveBlockRight = Animator.StringToHash("LeaveBlockRight");
        private static readonly int leaveBlockLeft  = Animator.StringToHash("LeaveBlockLeft");
        private static readonly int stuned = Animator.StringToHash("Stunned");

        private bool _combatLayerActive = false;
        private bool _isSheathing = false;
        private bool _swordInHand = false;
        private float _stunTimer = 0f;

       
        public SyncVar<bool> isBlocking = new SyncVar<bool>(false, ownerAuth: true);
        public bool isStunned => _stunTimer > 0f;

        protected override void OnSpawned()
        {
            if (!isOwner) enabled = false;
        }

        public void BreakBlock()
        {
            if (!isBlocking.value) return;

            isBlocking.value = false;
            animator?.SetBool(block, false);
            _stunTimer = blockBreakStunDuration;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                _swordInHand = !_swordInHand;
            }
            

            float currentWeight = animator != null ? animator.GetLayerWeight(1) : 0f;
            if (_isSheathing)
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(1);
                bool sheathDone = stateInfo.IsName("SheathSword") && stateInfo.normalizedTime >= .7f;
                print(sheathDone);
                if (sheathDone)
                {
                    animator?.SetLayerWeight(1, Mathf.MoveTowards(currentWeight, 0f, Time.deltaTime * 5f));
                    if (currentWeight <= 0f) _isSheathing = false;
                }
            }
            else if (_swordInHand)
            {
                animator?.SetLayerWeight(1, Mathf.MoveTowards(currentWeight, 1f, Time.deltaTime * 5f));
            }
            
            if (_swordInHand && !_combatLayerActive)
            {
                _combatLayerActive = true;
                animator?.SetTrigger(DrawSword);
                _isSheathing = false;
            }
            else if (!_swordInHand && _combatLayerActive)
            {
                _combatLayerActive = false;
                animator?.SetTrigger(SheathSword);
                _isSheathing = true;
            }
            

            if (!_swordInHand) return;
            if (Input.GetKey(KeyCode.I))
            {
                isBlocking.value = true;
                animator?.SetTrigger(block);
            }
                
            if (Input.GetMouseButtonDown(1))
            {
                isBlocking.value = true;
                animator?.SetTrigger(block);
            }
            if (Input.GetMouseButtonUp(1) && isBlocking)
            {
                isBlocking.value = false;
                animator?.SetBool(block, false);

                if (_side == "LEFT")
                    animator?.SetTrigger(leaveBlockLeft);
                else if (_side == "RIGHT")
                    animator?.SetTrigger(leaveBlockRight);
            }
            if (IsSwingPlaying())
            {
                return;
            }
            
            if (Input.GetMouseButtonDown(0) && !isBlocking.value)
            {
                // print(_side);
                if (_side == "RIGHT")
                {
                    // print("HERE");
                    animator?.SetTrigger(RightSwing);
                    _side = "LEFT";
                }
                else if (_side == "LEFT")
                {
                    animator?.SetTrigger(LeftSwing);
                    _side = "RIGHT";
                }
                else if (_side == "DOWN")
                {
                    animator?.SetTrigger(DownSwing);
                }
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                animator?.SetTrigger(punch);
            }
        }

        private bool IsSwingPlaying()
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(1);
            return (stateInfo.IsName("Armature_RightSwingAttack") || stateInfo.IsName("Armature_LeftSwingAttack") || stateInfo.IsName("Armature_DownSwingAttack"));
        }
        
        public void handleStun()
        {
            isBlocking.value = false;
            animator?.SetTrigger(stuned);
            _side = "Right";

        }
    }
}