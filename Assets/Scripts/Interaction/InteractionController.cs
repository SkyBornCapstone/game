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
                    new Vector3(head.transform.forward.x, _cam.transform.forward.y, head.transform.forward.z),
                    out RaycastHit hit, interactionRange,
                    interactableLayer))
                return;

            var interactable = hit.collider.GetComponent<AInteractable>();
            if (interactable && interactable.CanInteract(this))
                interactable.Interact(this);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawLine(head.position,
                head.position +
                new Vector3(head.transform.forward.x, _cam.transform.forward.y, head.transform.forward.z) *
                interactionRange);
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