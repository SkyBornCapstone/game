using PurrNet;
using UnityEngine;

namespace Interaction
{
    public class InteractionController : NetworkBehaviour
    {
        [SerializeField] private LayerMask interactableLayer;
        [SerializeField] private float interactionRange = 5f;
        [SerializeField] private Transform rightHandTarget;

        private Camera _cam;
        private AInteractable _hoveredInteractable;

        protected override void OnSpawned()
        {
            _cam = Camera.main;
            enabled = isOwner;
        }

        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.E))
                return;

            if (!Physics.Raycast(_cam.transform.position, _cam.transform.forward, out RaycastHit hit, interactionRange,
                    interactableLayer))
                return;

            var interactable = hit.collider.GetComponent<AInteractable>();
            if (interactable && interactable.CanInteract())
                interactable.Interact(this);
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

        public virtual bool CanInteract()
        {
            return true;
        }
    }
}