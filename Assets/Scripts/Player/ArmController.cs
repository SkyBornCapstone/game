using UnityEngine;

namespace Player
{
    public class ArmIKController : MonoBehaviour
    {
        [SerializeField] private Animator animator;

        // Assign these in the Inspector
        [SerializeField] private Transform leftHandTarget;
        [SerializeField] private Transform rightHandTarget;

        [Header("Grabbing")] [SerializeField] private float grabDistance = 3f;
        [SerializeField] private LayerMask grabbableMask = ~0; // default to everything

        private Transform _heldLeft;
        private Transform _heldRight;

        private float _leftHandWeight;
        private float _rightHandWeight;

        // How fast the hands transition
        [SerializeField] private float ikTransitionSpeed = 4f;

        void Update()
        {
            // Toggle grab/release on click for each hand
            if (Input.GetMouseButtonDown(0))
            {
                if (_heldLeft != null)
                    Release(ref _heldLeft);
                else
                    TryGrab(ref _heldLeft, leftHandTarget);
            }

            if (Input.GetMouseButtonDown(1))
            {
                if (_heldRight != null)
                    Release(ref _heldRight);
                else
                    TryGrab(ref _heldRight, rightHandTarget);
            }

            // IK only while holding something; keep arms out until released
            float leftTargetWeight = _heldLeft != null ? 1.0f : 0.0f;
            float rightTargetWeight = _heldRight != null ? 1.0f : 0.0f;

            _leftHandWeight = Mathf.Lerp(_leftHandWeight, leftTargetWeight, Time.deltaTime * ikTransitionSpeed);
            _rightHandWeight = Mathf.Lerp(_rightHandWeight, rightTargetWeight, Time.deltaTime * ikTransitionSpeed);
        }


        // Generic helpers to reduce duplication
        private void TryGrab(ref Transform held, Transform handTarget)
        {
            if (handTarget == null || held != null)
                return;

            Ray ray = Camera.main != null
                ? Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0))
                : new Ray(Vector3.zero, Vector3.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, grabDistance, grabbableMask, QueryTriggerInteraction.Ignore))
            {
                var grabbable = hit.transform.GetComponentInParent<Grabbable>();
                if (grabbable != null)
                {
                    held = grabbable.transform;
                    held.SetParent(handTarget, worldPositionStays: false);
                    held.localPosition = Vector3.zero;
                    held.localRotation = Quaternion.identity;

                    var rb = held.GetComponent<Rigidbody>();
                    if (rb != null) rb.isKinematic = true;
                }
            }
        }

        private void Release(ref Transform held)
        {
            if (held == null) return;
            var rb = held.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = false;
            held.SetParent(null, true);
            held = null;
        }

        private void ApplyIK(AvatarIKGoal goal, Transform target, float weight)
        {
            if (animator == null || target == null)
                return;

            animator.SetIKPositionWeight(goal, weight);
            animator.SetIKRotationWeight(goal, weight);
            animator.SetIKPosition(goal, target.position);
            animator.SetIKRotation(goal, target.rotation);
        }

        // This callback runs *after* the animation (like walking) is calculated,
        // but *before* the frame is rendered. It's the perfect place for IK.
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