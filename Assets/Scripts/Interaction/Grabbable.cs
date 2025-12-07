using PurrNet.Prediction;
using UnityEngine;
using UnityEngine.Animations;

namespace Interaction
{
    /// <summary>
    /// Tag component to mark items as grabbable.
    /// Add this to any GameObject (or one of its parents) to allow the player to grab it.
    /// </summary>
    [RequireComponent(typeof(PredictedRigidbody))]
    [RequireComponent(typeof(ParentConstraint))]
    public class Grabbable : PredictedIdentity<Grabbable.GrabbableState>
    {
        private PredictedRigidbody _rb;
        private ParentConstraint _parentConstraint;
        private bool _constraintApplied;

        private void Awake()
        {
            _rb = GetComponent<PredictedRigidbody>();
            _parentConstraint = GetComponent<ParentConstraint>();

            _parentConstraint.enabled = false;
        }

        protected override void Simulate(ref GrabbableState state, float delta)
        {
            _rb.isKinematic = state.IsGrabbed;
        }

        protected override void UpdateView(GrabbableState viewState, GrabbableState? verified)
        {
            bool shouldBeConstrained = viewState.IsGrabbed;

            if (shouldBeConstrained && !_constraintApplied)
            {
                _parentConstraint.enabled = true;
                _parentConstraint.constraintActive = true;
                _constraintApplied = true;
            }
            else if (!shouldBeConstrained && _constraintApplied)
            {
                _parentConstraint.enabled = false;
                _parentConstraint.constraintActive = false;
                _parentConstraint.RemoveSource(0);
                _constraintApplied = false;
            }
        }

        public void SetConstraintSource(Transform handTarget)
        {
            if (_parentConstraint.sourceCount != 0) return;
            var source = new ConstraintSource { sourceTransform = handTarget, weight = 1f };
            _parentConstraint.AddSource(source);
        }

        protected override GrabbableState GetInitialState()
        {
            return new GrabbableState { IsGrabbed = false };
        }

        public struct GrabbableState : IPredictedData<GrabbableState>
        {
            public bool IsGrabbed;

            public void Dispose()
            {
            }
        }
    }
}