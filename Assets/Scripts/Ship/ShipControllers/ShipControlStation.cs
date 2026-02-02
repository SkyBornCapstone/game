using Interaction;
using Player;
using PurrNet;
using UnityEngine;

namespace Ship.ShipControllers
{
    public abstract class ShipControlStation : AInteractable
    {
        [Header("Station Settings")] [SerializeField]
        protected Transform seatPosition;

        private readonly SyncVar<PlayerMovement> _occupyingPlayer = new();

        protected virtual void Update()
        {
            if (_occupyingPlayer.value && _occupyingPlayer.value.isOwner)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                }

                HandleInput();
            }
        }

        public override void Interact(InteractionController interactionController)
        {
            if (_occupyingPlayer.value && _occupyingPlayer.value.owner == interactionController.owner)
            {
                _occupyingPlayer.value.SetLockedPosition(null);
                LeaveStation();
            }
            else if (interactionController.TryGetComponent(out PlayerMovement player))
            {
                player.SetLockedPosition(seatPosition);
                EnterStation(player);
            }
        }

        [ServerRpc]
        protected virtual void EnterStation(PlayerMovement player)
        {
            _occupyingPlayer.value = player;
        }

        [ServerRpc]
        protected virtual void LeaveStation()
        {
            if (_occupyingPlayer.value)
            {
                _occupyingPlayer.value = null;
            }
        }

        protected abstract void HandleInput();

        public override bool CanInteract(InteractionController interactionController)
        {
            interactionController.TryGetComponent(out PlayerMovement playerMovement);
            if (playerMovement == _occupyingPlayer.value) return true;

            return !_occupyingPlayer.value;
        }
    }
}