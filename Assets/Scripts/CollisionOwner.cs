using Player;
using PurrNet;
using UnityEngine;

public class CollisionOwner : NetworkBehaviour
{
    protected override void OnSpawned(bool asServer)
    {
        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = !isOwner && !asServer;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.TryGetComponent<PlayerMovement>(out var playerMovement))
        {
            GiveOwnership(playerMovement.owner);
        }
    }

    protected override void OnOwnerChanged(PlayerID? oldOwner, PlayerID? newOwner, bool asServer)
    {
        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = !isOwner && !asServer;
    }
}