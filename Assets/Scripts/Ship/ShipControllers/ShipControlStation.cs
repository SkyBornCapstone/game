using Interaction;
using Player;
using UnityEngine;

namespace Ship.ShipControllers
{
    public abstract class ShipControlStation : AInteractable
    {
        [Header("Station Settings")] [SerializeField]
        protected Transform seatPosition;

        private PlayerMovement _occupyingPlayer;

        protected virtual void Update()
        {
            if (isOwner) HandleInput();
        }

        public override void Interact(InteractionController interactionController)
        {
            if (isOwner && _occupyingPlayer)
            {
                _occupyingPlayer.SetLockedPosition(null);
                _occupyingPlayer = null;
                RemoveOwnership();
            }
            else if (!hasOwner && !_occupyingPlayer && interactionController.TryGetComponent(out PlayerMovement player))
            {
                GiveOwnership(player.localPlayer);
                player.SetLockedPosition(seatPosition);
                _occupyingPlayer = player;
            }
        }

        protected abstract void HandleInput();

        public override bool CanInteract(InteractionController interactionController)
        {
            interactionController.TryGetComponent(out PlayerMovement playerMovement);
            if (playerMovement == _occupyingPlayer) return true;

            return !_occupyingPlayer;
        }
    }
}