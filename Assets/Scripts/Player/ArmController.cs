using UnityEngine;

namespace Player
{
    public class ArmIKController : MonoBehaviour
    {
        [SerializeField] private Animator animator;

        // Assign these in the Inspector
        [SerializeField]
        private Transform leftHandTarget;
        [SerializeField]
        private Transform rightHandTarget;

        // How quickly the hands move to/from the target (0-1)
        [Range(0, 1)] [SerializeField]
        private float leftHandWeight;
        [Range(0, 1)] [SerializeField]
        private float rightHandWeight;

        // How fast the hands transition
        [SerializeField]
        private float ikTransitionSpeed = 4f;

        void Update()
        {
            // --- Left Hand (Left Click) ---
            // Check if the left mouse button is being held down
            bool isLeftClick = Input.GetMouseButton(0);

            // Smoothly increase weight to 1 if clicked, or decrease to 0 if not
            float leftTargetWeight = isLeftClick ? 1.0f : 0.0f;
            leftHandWeight = Mathf.Lerp(leftHandWeight, leftTargetWeight, Time.deltaTime * ikTransitionSpeed);

            // --- Right Hand (Right Click) ---
            // Check if the right mouse button is being held down
            bool isRightClick = Input.GetMouseButton(1);

            // Smoothly increase weight to 1 if clicked, or decrease to 0 if not
            float rightTargetWeight = isRightClick ? 1.0f : 0.0f;
            rightHandWeight = Mathf.Lerp(rightHandWeight, rightTargetWeight, Time.deltaTime * ikTransitionSpeed);
        }

        // This callback runs *after* the animation (like walking) is calculated,
        // but *before* the frame is rendered. It's the perfect place for IK.
        void OnAnimatorIK(int layerIndex)
        {
            if (animator)
            {
                // --- Left Hand Control ---
                if (leftHandTarget != null)
                {
                    
                    // Set the *weight* (influence) of the IK
                    animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftHandWeight);
                    animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, leftHandWeight);
                
                    // Set the *target* position and rotation for the hand
                    animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTarget.position);
                    animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandTarget.rotation);
                }

                // --- Right Hand Control ---
                if (rightHandTarget != null)
                {
                    // Set the *weight* (influence) of the IK
                    animator.SetIKPositionWeight(AvatarIKGoal.RightHand, rightHandWeight);
                    animator.SetIKRotationWeight(AvatarIKGoal.RightHand, rightHandWeight);

                    // Set the *target* position and rotation for the hand
                    animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandTarget.position);
                    animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandTarget.rotation);
                }
            }
        }
    }
}