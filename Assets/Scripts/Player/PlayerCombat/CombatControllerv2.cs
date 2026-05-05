using Interaction;
using PurrNet;
using UnityEngine;

namespace Player.PlayerCombat
{
    public class CombatControllerv2 : NetworkBehaviour
    {
        [Header("References")] [SerializeField]
        public LockOnSystem lockOnSystem;

        [SerializeField] public GrabController grabController;
        [SerializeField] public NetworkAnimator animator;

        [Header("Stance Settings")] [SerializeField]
        private float blockBreakStunDuration = 1.5f;

        [SerializeField] public string _side = "RIGHT";

        private static readonly int RightSwing = Animator.StringToHash("RightSwing");
        private static readonly int LeftSwing = Animator.StringToHash("LeftSwing");
        private static readonly int DownSwing = Animator.StringToHash("DownSwing");
        private static readonly int LeftStance = Animator.StringToHash("LeftStance");
        private static readonly int RightStance = Animator.StringToHash("RightStance");
        private static readonly int DownStance = Animator.StringToHash("DownStance");
        private static readonly int DrawSword = Animator.StringToHash("DrawSword");
        private static readonly int SheathSword = Animator.StringToHash("SheathSword");
        private static readonly int punch = Animator.StringToHash("Punch");
        private static readonly int block = Animator.StringToHash("Block");
        private static readonly int leaveBlockRight = Animator.StringToHash("LeaveBlockRight");
        private static readonly int leaveBlockLeft = Animator.StringToHash("LeaveBlockLeft");
        private static readonly int stuned = Animator.StringToHash("Stunned");

        private bool _combatLayerActive = false;
        private bool _isSheathing = false;
        private bool _swordInHand = false;
        private float _stunTimer = 0f;

        // Renamed to _isBlocking, exposed via read-only property
        private SyncVar<bool> _isBlocking = new SyncVar<bool>(false, ownerAuth: true);
        public bool isBlocking => _isBlocking.value;
        public bool isStunned => _stunTimer > 0f;

        private PlayerSounds _playerSounds;

        private void Awake()
        {
            _playerSounds = GetComponent<PlayerSounds>();
        }

        protected override void OnSpawned()
        {
            if (!isOwner) enabled = false;
        }

        // Only the owner can set this
        private void SetBlocking(bool value)
        {
            if (!isOwner) return;
            _isBlocking.value = value;
        }

        public void BreakBlock()
        {
            if (!isOwner) return;
            if (!isBlocking) return;

            SetBlocking(false);
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
                if (sheathDone)
                {
                    animator?.SetLayerWeight(1, Mathf.MoveTowards(currentWeight, 0f, Time.deltaTime * 5f));
                    if (currentWeight <= 0f) _isSheathing = false;
                }
            }
            else if (_swordInHand)
            {
                animator?.SetLayerWeight(1, Mathf.MoveTowards(currentWeight, 1f, Time.deltaTime * 5f));
                grabController.Drop();
            }

            if (_swordInHand && !_combatLayerActive)
            {
                _combatLayerActive = true;
                animator?.SetTrigger(DrawSword);
                _isSheathing = false;
                
                if (_playerSounds != null) _playerSounds.PlaySwordUnsheathe();
            }
            else if (!_swordInHand && _combatLayerActive)
            {
                _combatLayerActive = false;
                animator?.SetTrigger(SheathSword);
                _isSheathing = true;

                if (_playerSounds != null) _playerSounds.PlaySwordSheathe();
            }

            if (!_swordInHand) return;

            if (Input.GetKey(KeyCode.I))
            {
                SetBlocking(true);
                animator?.SetTrigger(block);
            }

            if (Input.GetMouseButtonDown(1))
            {
                SetBlocking(true);
                animator?.SetTrigger(block);
            }

            if (Input.GetMouseButtonUp(1) && isBlocking)
            {
                SetBlocking(false);
                animator?.SetBool(block, false);

                if (_side == "LEFT")
                    animator?.SetTrigger(leaveBlockLeft);
                else if (_side == "RIGHT")
                    animator?.SetTrigger(leaveBlockRight);
            }

            if (IsSwingPlaying()) return;

            if (Input.GetMouseButtonDown(0) && !isBlocking)
            {
                if (_side == "RIGHT")
                {
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

                if (_playerSounds != null) _playerSounds.PlaySwordSwing();
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                animator?.SetTrigger(punch);
            }
        }

        private bool IsSwingPlaying()
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(1);
            return stateInfo.IsName("Armature_RightSwingAttack")
                   || stateInfo.IsName("Armature_LeftSwingAttack")
                   || stateInfo.IsName("Armature_DownSwingAttack");
        }

        public void handleStun()
        {
            if (!isOwner) return;
            SetBlocking(false);
            animator?.SetTrigger(stuned);
            _side = "RIGHT";
        }
    }
}