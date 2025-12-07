using Interaction;
using PurrNet.Prediction;
using UnityEngine;

namespace Player
{
    public class GrabController : PredictedIdentity<ArmIkInput, ArmIKState>
    {
        [SerializeField] private ArmIKController armIKController;
        [SerializeField] private Transform leftHandTarget;
        [SerializeField] private Transform rightHandTarget;

        [SerializeField] private float grabDistance = 3f;
        [SerializeField] private LayerMask grabbableMask = ~0;

        protected override void Simulate(ArmIkInput input, ref ArmIKState state, float delta)
        {
            if (input.grabLeftPressed)
            {
                if (state.heldLeftId.HasValue)
                {
                    UpdateGrabbableState(state.heldLeftId.Value, false);
                    state.heldLeftId = null;
                }
                else if (input.raycastHitId.HasValue)
                {
                    state.heldLeftId = input.raycastHitId;
                    UpdateGrabbableState(input.raycastHitId.Value, true);
                    SetConstraintSource(input.raycastHitId.Value, leftHandTarget);
                }
            }
            else if (input.grabRightPressed)
            {
                if (state.heldRightId.HasValue)
                {
                    UpdateGrabbableState(state.heldRightId.Value, false);
                    state.heldRightId = null;
                }
                else if (input.raycastHitId.HasValue)
                {
                    state.heldRightId = input.raycastHitId;
                    UpdateGrabbableState(input.raycastHitId.Value, true);
                    SetConstraintSource(input.raycastHitId.Value, rightHandTarget);
                }
            }
        }

        private void UpdateGrabbableState(PredictedObjectID grabbableId, bool isGrabbed)
        {
            if (predictionManager.hierarchy.TryGetComponent<Grabbable>(grabbableId, out var grabbable))
            {
                grabbable.currentState.IsGrabbed = isGrabbed;
            }
        }

        private void SetConstraintSource(PredictedObjectID grabbableId, Transform handTarget)
        {
            if (predictionManager.hierarchy.TryGetComponent<Grabbable>(grabbableId, out var grabbable))
            {
                grabbable.SetConstraintSource(handTarget);
            }
        }

        protected override void UpdateInput(ref ArmIkInput input)
        {
            if (Input.GetMouseButtonDown(0))
            {
                input.grabLeftPressed = true;
                input.raycastHitId = PerformGrabRaycast();
            }

            if (Input.GetMouseButtonDown(1))
            {
                input.grabRightPressed = true;
                input.raycastHitId = PerformGrabRaycast();
            }
        }

        private PredictedObjectID? PerformGrabRaycast()
        {
            var ray = Camera.main is not null
                ? Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0))
                : new Ray(Vector3.zero, Vector3.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, grabDistance, grabbableMask, QueryTriggerInteraction.Ignore))
            {
                var grabbable = hit.transform.GetComponentInParent<Grabbable>();
                if (grabbable is not null && !grabbable.currentState.IsGrabbed &&
                    predictionManager.hierarchy.TryGetId(grabbable.gameObject, out var netId))
                {
                    return netId;
                }
            }

            return null;
        }

        protected override void UpdateView(ArmIKState viewState, ArmIKState? verified)
        {
            if (!verified.HasValue) return;

            if (verified.Value.heldLeftId is not null)
            {
                var grabbable = verified.Value.heldLeftId.Value.GetComponent<PredictedTransform>(predictionManager);
                armIKController.leftHandTarget = grabbable.graphics;
            }
            else
            {
                armIKController.leftHandTarget = null;
            }

            if (verified.Value.heldRightId is not null)
            {
                var grabbable = verified.Value.heldRightId.Value.GetComponent<PredictedTransform>(predictionManager);
                armIKController.rightHandTarget = grabbable.graphics;
            }
            else
            {
                armIKController.rightHandTarget = null;
            }
        }
    }

    public struct ArmIKState : IPredictedData<ArmIKState>
    {
        public PredictedObjectID? heldLeftId;
        public PredictedObjectID? heldRightId;

        public void Dispose()
        {
        }
    }

    public struct ArmIkInput : IPredictedData
    {
        public bool grabLeftPressed;
        public bool grabRightPressed;
        public PredictedObjectID? raycastHitId;

        public void Dispose()
        {
        }
    }
}