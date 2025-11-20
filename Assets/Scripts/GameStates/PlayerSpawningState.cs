using System.Collections.Generic;
using PurrNet.Prediction;
using PurrNet.Prediction.StateMachine;
using UnityEngine;

namespace GameStates
{
    public class PlayerSpawningState : PredictedStateNode<PlayerSpawningState.State>
    {
        public GameObject playerPrefab;
        public List<Transform> spawnPoints = new();

        public override void Enter()
        {
            for (var i = 0; i < predictionManager.players.currentState.players.Count; i++)
            {
                var player = predictionManager.players.currentState.players[i];
                var spawnPoint = spawnPoints[i];
                predictionManager.hierarchy.Create(playerPrefab, spawnPoint.position, spawnPoint.rotation, player);
            }

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