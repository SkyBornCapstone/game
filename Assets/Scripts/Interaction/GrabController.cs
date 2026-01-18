using Player;
using PurrNet;
using UnityEngine;

namespace Interaction
{
    public class GrabController : NetworkBehaviour
    {
        [SerializeField] private ArmIKController armIKController;
        [SerializeField] private Transform leftHandTarget;
        [SerializeField] private Transform rightHandTarget;

        private Grabbable _currentGrabbed;

        public bool IsGrabbing => _currentGrabbed != null;

        protected override void OnSpawned()
        {
            enabled = isOwner;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q) && IsGrabbing)
            {
                _currentGrabbed.Drop(this);
                armIKController.rightHandTarget.value = null;
                _currentGrabbed = null;
            }

            if (Input.GetMouseButtonDown(0) && IsGrabbing)
            {
                _currentGrabbed.Use();
            }
        }

        public void Grab(Grabbable grabbable)
        {
            grabbable.GiveOwnership(owner);
            _currentGrabbed = grabbable;
            grabbable.SetConstraintSource(rightHandTarget);

            armIKController.rightHandTarget.value = rightHandTarget;
        }
    }
}