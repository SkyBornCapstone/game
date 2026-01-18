using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

namespace Interaction
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(ParentConstraint))]
    public class Grabbable : AInteractable
    {
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
            if (grabController == null || grabController.IsGrabbing) return;

            SetPhysics(true);
            grabController.Grab(this);
        }

        public virtual void Drop(GrabController grabController)
        {
            _parentConstraint.constraintActive = false;
            _parentConstraint.SetSources(new List<ConstraintSource>());
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


        public void SetConstraintSource(Transform handTarget)
        {
            _parentConstraint.enabled = true;
            _parentConstraint.constraintActive = true;
            if (_parentConstraint.sourceCount != 0) return;
            var source = new ConstraintSource { sourceTransform = handTarget, weight = 1f };
            _parentConstraint.AddSource(source);
        }
    }
}