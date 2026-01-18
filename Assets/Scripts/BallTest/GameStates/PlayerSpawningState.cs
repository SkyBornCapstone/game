using System.Collections.Generic;
using PurrNet;
using PurrNet.StateMachine;
using UnityEngine;

namespace BallTest.GameStates
{
    public class PlayerSpawningState : StateNode
    {
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private List<Transform> spawnPoints = new();

        public override void Enter()
        {
            for (var i = 0; i < networkManager.players.Count; i++)
            {
                var player = networkManager.players[i];
                var spawnPoint = spawnPoints[i];
                var spawnedPlayer = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
                spawnedPlayer.TryGetComponent(out NetworkIdentity networkIdentity);
                networkIdentity.GiveOwnership(player);
            }

            machine.Next();
        }
    }
}