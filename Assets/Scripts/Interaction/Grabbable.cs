using UnityEngine;
using UnityEngine.Animations;

namespace Interaction
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(ParentConstraint))]
    public class Grabbable : AInteractable
    {
        public Transform rightHandGrip;

        private Rigidbody _rb;
        private ParentConstraint _parentConstraint;
        private bool _constraintApplied;

        protected override void OnSpawned()
        {
            _rb = GetComponent<Rigidbody>();
            _parentConstraint = GetComponent<ParentConstraint>();

            _parentConstraint.constraintActive = false;
        }

        public override void Interact(InteractionController interactionController)
        {
            var grabController = interactionController.GetComponent<GrabController>();
            Grab(grabController);
        }

        public virtual void Grab(GrabController grabController)
        {
            if (grabController == null || grabController.isGrabbing.value) return;

            SetPhysics(true);
            grabController.Grab(this);
        }

        public virtual void Drop(GrabController grabController)
        {
            SetPhysics(false);
        }

        public virtual void Use()
        {
            Debug.Log("Used");
        }

        public void SetPhysics(bool grabbed)
        {
            _rb.isKinematic = grabbed;
        }
    }
}