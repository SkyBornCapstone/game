using PurrNet;
using UnityEngine;

namespace Interaction
{
    public class InteractionController : NetworkBehaviour
    {
        [SerializeField] private LayerMask interactableLayer;
        [SerializeField] private float interactionRange = 5f;
        [SerializeField] private Transform rightHandTarget;
        [SerializeField] private Transform head;

        private Camera _cam;
        private AInteractable _hoveredInteractable;

        protected override void OnSpawned()
        {
            _cam = Camera.main;
            enabled = isOwner;
        }

        private void Update()
        {
            if (!isOwner) return;

            if (!Input.GetKeyDown(KeyCode.E))
                return;

            if (!Physics.Raycast(head.position,
                    GetProxyForward(),
                    out RaycastHit hit, interactionRange,
                    interactableLayer))
                return;

            var interactable = hit.collider.GetComponent<AInteractable>();
            if (interactable && interactable.CanInteract(this))
                interactable.Interact(this);
        }

        private void OnDrawGizmosSelected()
        {
            if (_cam)
                Gizmos.DrawLine(head.position, head.position + GetProxyForward() * interactionRange);
        }

        private Vector3 GetProxyForward()
        {
            float cameraPitch = _cam.transform.localEulerAngles.x;
            Quaternion pitchRotation = Quaternion.Euler(cameraPitch, 0, 0);
            return head.rotation * pitchRotation * Vector3.forward;
        }
    }

    public abstract class AInteractable : NetworkBehaviour
    {
        public abstract void Interact(InteractionController interactionController);

        public virtual void OnHover()
        {
        }

        public virtual void OnStopHover()
        {
        }

        public virtual bool CanInteract(InteractionController interactionController)
        {
            return true;
        }
    }
}