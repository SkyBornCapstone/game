using System;
using UnityEngine;

using Player;

namespace Cannons
{
    public class CannonInteract : MonoBehaviour
    {
        public Transform controlPosition;

        public float interactDistance = 1f;
        [SerializeField] private CannonController cannonController;
        private PlayerMovement _currentPlayer;
        
        // Update is called once per frame
        void Update()
        {
            if (!_currentPlayer) return;

            if (Input.GetKeyDown(KeyCode.E))
            {
                if (!_currentPlayer.isUsingCannon)
                {
                    _currentPlayer.EnterCannon(controlPosition);
                    cannonController.EnterCannon(_currentPlayer);
                }
                else
                {
                    _currentPlayer.ExitCannon();
                    cannonController.ExitCannon();
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out PlayerMovement player))
            {
                _currentPlayer = player;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out PlayerMovement player))
            {
                if (player == _currentPlayer)
                {
                    _currentPlayer = null;
                }
                
            }
        }
    }
}

