using UnityEngine;

namespace Player
{
    public class ArmIKController : MonoBehaviour
    {
        [SerializeField] private Animator animator;

        [SerializeField] private float ikTransitionSpeed = 4f;

        [SerializeField] public Transform leftHandTarget;
        [SerializeField] public Transform rightHandTarget;

        private float _leftHandWeight;
        private float _rightHandWeight;

        private void Update()
        {
            var leftTargetWeight = leftHandTarget is not null ? 1.0f : 0.0f;
            var rightTargetWeight = rightHandTarget is not null ? 1.0f : 0.0f;

            _leftHandWeight = Mathf.Lerp(_leftHandWeight, leftTargetWeight, Time.deltaTime * ikTransitionSpeed);
            _rightHandWeight = Mathf.Lerp(_rightHandWeight, rightTargetWeight, Time.deltaTime * ikTransitionSpeed);
        }

        private void ApplyIK(AvatarIKGoal goal, Transform target, float weight)
        {
            animator.SetIKPositionWeight(goal, weight);
            animator.SetIKRotationWeight(goal, weight);

            if (animator == null || target == null)
                return;

            animator.SetIKPosition(goal, target.position);
            animator.SetIKRotation(goal, target.rotation);
        }

        void OnAnimatorIK(int layerIndex)
        {
            if (animator)
            {
                ApplyIK(AvatarIKGoal.LeftHand, leftHandTarget, _leftHandWeight);
                ApplyIK(AvatarIKGoal.RightHand, rightHandTarget, _rightHandWeight);
            }
        }
    }
}