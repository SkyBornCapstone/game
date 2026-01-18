using PurrNet.StateMachine;
using UnityEngine;

namespace GameStates
{
    public class WaitForPlayersState : StateNode
    {
        [SerializeField] private int requiredPlayers = 2;

        public override void StateUpdate()
        {
            if (!isServer) return;

            if (networkManager.players.Count >= requiredPlayers)
                machine.Next();
        }
    }
}