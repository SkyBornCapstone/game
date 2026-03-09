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

        private static readonly int RightSwing = Animator.StringToHash("RightSwing");

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
                armIKController.rightHandTargetSwing.value = null;
                
                return;
            }

            bool holdingLeftClick = Input.GetMouseButtonDown(0);
            if (holdingLeftClick)
            {
                print("HERE");
                animator?.SetTrigger(RightSwing);
            }

        }
    }
}