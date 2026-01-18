using PurrNet.StateMachine;
using UnityEngine;

namespace BallTest.GameStates
{
    public class WaitForPlayersState : StateNode
    {
        [SerializeField] private int requiredPlayers = 2;

        public override void StateUpdate()
        {
            if (networkManager.players.Count >= requiredPlayers)
                machine.Next();
        }
    }
}