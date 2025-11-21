using PurrNet;
using PurrNet.Pooling;
using PurrNet.Prediction;
using PurrNet.Prediction.StateMachine;

namespace GameStates
{
    public class RoundRunningState : PredictedStateNode<RoundRunningState.State>
    {
        private void Awake()
        {
            // TODO Integrate with health system to end when dead
        }

        protected override void OnDestroy()
        {
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