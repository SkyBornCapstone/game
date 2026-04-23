using PurrNet;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Interaction
{
    public class GrabController : NetworkBehaviour
    {
        [SerializeField] private Transform leftHandTarget;
        [SerializeField] private Transform rightHandTarget;
        [SerializeField] private TwoBoneIKConstraint rightHandIK;
        [SerializeField] private Transform rightIkTarget;
        [SerializeField] private Transform rightHandVisualTarget;

        [SerializeField] private float ikTransitionSpeed = 4f;

        private Grabbable _currentGrabbed;

        public SyncVar<bool> isGrabbing = new(ownerAuth: true);

        protected override void OnSpawned()
        {
        }

        private void Update()
        {
            SetIKTargets();

            if (!isOwner) return;

            if (Input.GetKeyDown(KeyCode.Q) && isGrabbing.value)
            {
                _currentGrabbed.Drop(this);
                _currentGrabbed.transform.parent = null;
                _currentGrabbed = null;
                isGrabbing.value = false;
            }

            if (Input.GetMouseButtonDown(0) && isGrabbing.value)
            {
                _currentGrabbed.Use();
            }
        }

        private void LateUpdate()
        {
            if (!isOwner) return;

            if (isGrabbing.value)
            {
                Transform grabTransform = _currentGrabbed.transform;
                Transform gripPoint = _currentGrabbed.rightHandGrip;

                if (gripPoint)
                {
                    Quaternion inverseGripRot = Quaternion.Inverse(gripPoint.localRotation);
                    grabTransform.rotation = rightHandVisualTarget.rotation * inverseGripRot;

                    Vector3 offsetToRoot = grabTransform.position - gripPoint.position;

                    grabTransform.position = rightHandVisualTarget.position + offsetToRoot;
                }
            }
        }

        private void SetIKTargets()
        {
            var rightTargetWeight = isGrabbing ? 1.0f : 0.0f;

            rightHandIK.weight = Mathf.Lerp(rightHandIK.weight, rightTargetWeight, Time.deltaTime * ikTransitionSpeed);

            rightIkTarget.SetPositionAndRotation(rightHandTarget.position, rightHandTarget.rotation);
        }

        public void Grab(Grabbable grabbable)
        {
            grabbable.GiveOwnership(owner);
            _currentGrabbed = grabbable;
            isGrabbing.value = true;
        }
    }
}