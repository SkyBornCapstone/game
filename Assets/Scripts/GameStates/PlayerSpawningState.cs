using System.Collections.Generic;
using PurrNet;
using PurrNet.StateMachine;
using Unity.Cinemachine;
using UnityEngine;

namespace GameStates
{
    public class PlayerSpawningState : StateNode
    {
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private List<Transform> spawnPoints = new();
        [SerializeField] private CinemachineCamera firstPersonCamera;
        [SerializeField] private Canvas lobbyUI;
        [SerializeField] private Canvas gameUI;

        public override void Enter()
        {
            SetupClientUI();
            networkManager.FlushBatchedRPCs();

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

        [ObserversRpc(bufferLast: true, runLocally: true)]
        private void SetupClientUI()
        {
            firstPersonCamera.gameObject.SetActive(true);
            if (lobbyUI && gameUI)
            {
                lobbyUI.gameObject.SetActive(false);
                gameUI.gameObject.SetActive(true);
            }
        }
    }
}