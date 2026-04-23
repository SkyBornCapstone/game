using System;
using PurrNet;
using UnityEngine;

namespace Player.PlayerCombat
{
    public class CombatController : NetworkBehaviour
    {
        [Header("References")] [SerializeField]
        public LockOnSystem lockOnSystem;

        [SerializeField] public NetworkAnimator animator;

        [Header("Stance Settings")] [SerializeField]
        private float stanceSwitchThreshold = 1.5f;

        [SerializeField] private float stanceSwitchCooldown = 0.4f;

        public string _side = "LEFT";
        private float _lastStanceSwitchTime = -999f;

        private static readonly int RightSwing = Animator.StringToHash("RightSwing");
        private static readonly int LeftSwing = Animator.StringToHash("LeftSwing");
        private static readonly int DownSwing = Animator.StringToHash("DownSwing");
        private static readonly int LeftStance = Animator.StringToHash("LeftStance");
        private static readonly int RightStance = Animator.StringToHash("RightStance");
        private static readonly int DownStance = Animator.StringToHash("DownStance");
        private String prevStance;
        private static readonly int DrawSword = Animator.StringToHash("DrawSword");
        private static readonly int SheathSword = Animator.StringToHash("SheathSword");
        private bool _combatLayerActive = false;
        private bool _isSheathing = false;

        protected override void OnSpawned()
        {
            if (!isOwner) enabled = false;
        }

        private void Update()
        {
            bool isLockedOn = lockOnSystem.IsLockedOn;

            float currentWeight = animator != null ? animator.GetLayerWeight(1) : 0f;
            //print(currentWeight);
            if (_isSheathing)
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(1);
                Debug.Log(
                    $"Hash: {stateInfo.shortNameHash} | SheathHash: {SheathSword} | NormTime: {stateInfo.normalizedTime}");
                bool sheathDone = stateInfo.IsName("SheathSword") && stateInfo.normalizedTime >= .7f;
                print(sheathDone);
                if (sheathDone)
                {
                    animator?.SetLayerWeight(1, Mathf.MoveTowards(currentWeight, 0f, Time.deltaTime * 5f));
                    print("HERE");
                    if (currentWeight <= 0f) _isSheathing = false;
                }
            }
            else if (isLockedOn)
            {
                animator?.SetLayerWeight(1, Mathf.MoveTowards(currentWeight, 1f, Time.deltaTime * 5f));
            }

            // animator?.SetLayerWeight(1, Mathf.MoveTowards(currentWeight, isLockedOn ? 1f : 0f, Time.deltaTime * 5f));
            if (isLockedOn && !_combatLayerActive)
            {
                _combatLayerActive = true;
                animator?.SetTrigger(DrawSword);
                _isSheathing = false;
            }
            else if (!isLockedOn && _combatLayerActive)
            {
                _combatLayerActive = false;
                animator?.SetTrigger(SheathSword);
                _isSheathing = true;
            }


            if (!isLockedOn) return;
            if (IsSwingPlaying())
            {
                print("HEREASDDF SD FSDf");
                return;
            }

            bool canSwitch = Time.time - _lastStanceSwitchTime >= stanceSwitchCooldown;
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            if (canSwitch)
            {
                if ((Math.Abs(mouseY) > Math.Abs(mouseX) && mouseY > stanceSwitchThreshold) &&
                    (_side == "RIGHT" || _side == "LEFT"))
                {
                    prevStance = _side;
                    _side = "DOWN";
                    _lastStanceSwitchTime = Time.time;
                    animator?.SetTrigger(DownStance);
                }
                else if ((Math.Abs(mouseY) > Math.Abs(mouseX) && mouseY < -stanceSwitchThreshold) && _side == "DOWN")
                {
                    _side = prevStance;
                    _lastStanceSwitchTime = Time.time;
                    animator?.SetTrigger(_side == "RIGHT" ? RightStance : LeftStance);
                }
                else if (mouseX > stanceSwitchThreshold && (_side == "LEFT" || _side == "DOWN"))
                {
                    print(mouseX + "MOVE RIGHT");
                    prevStance = _side;
                    _side = "RIGHT";
                    _lastStanceSwitchTime = Time.time;
                    animator?.SetTrigger(RightStance);
                }
                else if (mouseX < -stanceSwitchThreshold && (_side == "RIGHT" || _side == "DOWN"))
                {
                    print("MOVE LEFT");
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
            }
        }

        private bool IsSwingPlaying()
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(1);
            return (stateInfo.IsName("Armature_RightSwingAttack") || stateInfo.IsName("Armature_LeftSwingAttack") ||
                    stateInfo.IsName("Armature_DownSwingAttack"));
        }
    }
}