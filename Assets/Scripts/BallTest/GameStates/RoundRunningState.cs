using System.Collections.Generic;
using PurrNet;
using PurrNet.StateMachine;

namespace BallTest.GameStates
{
    public class RoundRunningState : StateNode
    {
        private List<PlayerID> _currentPlayers;

        private void Awake()
        {
            PlayerHealth.OnDeath += OnPlayerDeath;
        }

        protected override void OnDestroy()
        {
            PlayerHealth.OnDeath -= OnPlayerDeath;
        }

        public override void Enter()
        {
            _currentPlayers = new List<PlayerID>(networkManager.players);
        }

        private void OnPlayerDeath(PlayerID? owner)
        {
            if (machine.currentStateNode is not RoundRunningState runningState || runningState != this)
                return;

            if (!owner.HasValue)
                return;

            _currentPlayers.Remove(owner.Value);

            if (_currentPlayers.Count <= 1)
            {
                machine.Next();
            }
        }
    }
}