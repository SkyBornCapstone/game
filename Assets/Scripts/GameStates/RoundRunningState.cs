using System.Collections.Generic;
using player;
using PurrNet;
using PurrNet.StateMachine;

namespace GameStates
{
    public class RoundRunningState : StateNode
    {
        private PlayerSpawningState _spawningState;
        private List<PlayerID> _currentPlayers;

        private void Awake()
        {
            PlayerHealth.OnDeath += OnPlayerDeath;
            TryGetComponent(out _spawningState);
        }

        protected override void OnDestroy()
        {
            PlayerHealth.OnDeath -= OnPlayerDeath;
        }

        public override void Enter(bool asServer)
        {
            if (!asServer) return;

            _currentPlayers = new List<PlayerID>(networkManager.players);
            networkManager.onPlayerJoined += OnPlayerJoined;
        }

        public override void Exit()
        {
            networkManager.onPlayerJoined -= OnPlayerJoined;
        }

        private void OnPlayerJoined(PlayerID player, bool isReconnect, bool asServer)
        {
            if (!asServer) return;

            var spawnPoint = _spawningState.spawnPoints[_currentPlayers.Count % _spawningState.spawnPoints.Count];
            var spawnedPlayer = Instantiate(_spawningState.playerPrefab, spawnPoint.position, spawnPoint.rotation);
            spawnedPlayer.TryGetComponent(out NetworkIdentity networkIdentity);
            networkIdentity.GiveOwnership(player);

            _currentPlayers.Add(player);
        }

        private void OnPlayerDeath(PlayerID? owner)
        {
            if (machine.currentStateNode is not RoundRunningState runningState || runningState != this)
                return;

            if (!owner.HasValue)
                return;

            _currentPlayers.Remove(owner.Value);

            if (_currentPlayers.Count <= 0)
            {
                machine.Next();
            }
        }
    }
}