using PurrNet.Prediction;
using PurrNet.Prediction.StateMachine;
using UnityEngine;

namespace GameStates
{
    public class WaitForPlayersState : PredictedStateNode<WaitForPlayersState.State>
    {
        [SerializeField] private int requiredPlayers = 2;

        protected override void StateSimulate(ref State state, float delta)
        {
            if (predictionManager.players.currentState.players.Count >= requiredPlayers)
                machine.Next();
        }

        public struct State : IPredictedData<State>
        {
            public void Dispose()
            {
            }
        }
    }
}