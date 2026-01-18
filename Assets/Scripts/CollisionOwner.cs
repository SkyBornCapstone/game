using Player;
using PurrNet;
using UnityEngine;

public class CollisionOwner : NetworkBehaviour
{
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.TryGetComponent<PlayerMovement>(out var playerMovement))
        {
            GiveOwnership(playerMovement.owner);
        }
    }
}