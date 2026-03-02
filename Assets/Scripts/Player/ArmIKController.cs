using PurrNet;
using UnityEngine;

namespace Player
{
    public class ArmIKController : NetworkBehaviour
    {
        [SerializeField] private NetworkAnimator animator;

        [SerializeField] private float ikTransitionSpeed = 4f;

        // [SerializeField] public Transform leftHandTarget;
        [SerializeField] public SyncVar<Transform> rightHandTargetSwing;
        [SerializeField] public SyncVar<Transform> rightHandTargetGrab;

        private float _leftHandWeight;
        private float _rightHandWeight;

        protected override void OnSpawned()
        {
            if (!isOwner)
                enabled = false;
        }

        private void Update()
        {
            // var leftTargetWeight = leftHandTarget is not null ? 1.0f : 0.0f;
            Transform activeTarget = rightHandTargetGrab.value != null 
                ? rightHandTargetGrab.value 
                : rightHandTargetSwing.value;
            var rightTargetWeight = activeTarget != null ? 1.0f : 0.0f;

            // _leftHandWeight = Mathf.Lerp(_leftHandWeight, leftTargetWeight, Time.deltaTime * ikTransitionSpeed);
            _rightHandWeight = Mathf.Lerp(_rightHandWeight, rightTargetWeight, Time.deltaTime * ikTransitionSpeed);
            // print($"{owner} {rightHandTarget.value} {rightTargetWeight} {_rightHandWeight}");
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
                Transform activeTarget = rightHandTargetGrab.value != null
                    ? rightHandTargetGrab.value
                    : rightHandTargetSwing.value;
                // ApplyIK(AvatarIKGoal.LeftHand, leftHandTarget, _leftHandWeight);
                ApplyIK(AvatarIKGoal.RightHand, activeTarget, _rightHandWeight);
            }
        }
    }
}