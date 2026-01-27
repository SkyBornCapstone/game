using Interaction;
using Player;
using UnityEngine;

namespace Cannons
{
    public class CannonInteract : AInteractable
    {
        [SerializeField] private Transform controlPosition;
        [SerializeField] private CannonController cannonController;

        private PlayerMovement _currentPlayer;

        public override void Interact(InteractionController interactionController)
        {
            if (!_currentPlayer)
            {
                _currentPlayer = interactionController.GetComponent<PlayerMovement>();
                GiveOwnership(_currentPlayer.owner);
                cannonController.EnterCannon(_currentPlayer);
                _currentPlayer.SetLockedPosition(controlPosition);
            }
            else if (_currentPlayer && isOwner)
            {
                _currentPlayer.SetLockedPosition(null);
                cannonController.ExitCannon();
                _currentPlayer = null;
                RemoveOwnership();
            }
        }
    }
}