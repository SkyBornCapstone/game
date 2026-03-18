using UnityEngine;
using PurrNet;

namespace Player.PlayerCombat
{
    public class ArmAimController : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] public LockOnSystem lockOnSystem;
        [SerializeField] public ArmIKController armIKController;
        [SerializeField] public Animator animator;
        public string _side = "LEFT";
        private static readonly int RightSwing = Animator.StringToHash("RightSwing");
        private static readonly int LeftSwing = Animator.StringToHash("LeftSwing");
        
        private static readonly int LeftStance = Animator.StringToHash("LeftStance");
        private static readonly int RightStance = Animator.StringToHash("RightStance");

        protected override void OnSpawned()
        {
            if (!isOwner) enabled = false;
        }

        private void Update()
        {

            bool isLockedOn = lockOnSystem.IsLockedOn;
            
            float currentWeight = animator != null ? animator.GetLayerWeight(1) : 0f;
            
            
            animator?.SetLayerWeight(1, Mathf.MoveTowards(currentWeight, isLockedOn ? 1f : 0f, Time.deltaTime * 5f));

            if (!isLockedOn)
            {
                // armIKController.rightHandTargetSwing.value = null;
                
                return;
            }
            bool holdingLeftClick = Input.GetMouseButtonDown(0);
            float mouseX = Input.GetAxis("Mouse X");
            if (mouseX > 2f && _side == "LEFT")
            {
                _side = "RIGHT";
                animator?.SetTrigger(RightStance);
            }else if (mouseX < -2f &&  _side == "RIGHT")
            {
                _side = "LEFT";
                animator?.SetTrigger(LeftStance);
            }
            
            if (holdingLeftClick && _side == "RIGHT")
            {
                
                animator?.SetTrigger(RightSwing);
            }
            else if (holdingLeftClick && _side == "LEFT")
            {
                animator?.SetTrigger(LeftSwing);
            }
            

        }
    }
}