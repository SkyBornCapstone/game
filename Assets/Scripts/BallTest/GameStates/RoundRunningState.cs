using PurrNet;
using PurrNet.Pooling;
using PurrNet.Prediction;
using PurrNet.Prediction.StateMachine;
using balltest;
namespace BallTest.GameStates
{
    public class RoundRunningState : PredictedStateNode<RoundRunningState.State>
    {
        private void Awake()
        {
            PlayerHealth.OnDeathAction += OnPlayerDeath;
        }

        protected override void OnDestroy()
        {
            PlayerHealth.OnDeathAction -= OnPlayerDeath;
        }

        public override void Enter()
        {
            currentState.Players = DisposableList<PlayerID>.Create(predictionManager.players.currentState.players);
        }

        private void OnPlayerDeath(PlayerID? owner)
        {
            if (machine.currentStateNode is not RoundRunningState runningState || runningState != this)
                return;

            if (!owner.HasValue)
                return;

            currentState.Players.Remove(owner.Value);

            if (currentState.Players.Count <= 1)
            {
                PlayerHealth.ClearPlayers.Invoke();
                machine.Next();
            }
        }

        public struct State : IPredictedData<State>
        {
            public DisposableList<PlayerID> Players;

            public void Dispose()
            {
                Players.Dispose();
            }
        }
    }
}