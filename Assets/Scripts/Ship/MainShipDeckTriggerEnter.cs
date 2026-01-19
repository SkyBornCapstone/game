using UnityEngine;
using Player;
using Ship;
using System.Collections.Generic;

public class MainShipDeckTriggerEnter : MonoBehaviour
{
    [Header("References")]
    public Transform deckProxy;        // ProxyShip transform

    private readonly HashSet<IShipProxyRider> riders = new();

    void Start()
    {
        Debug.Log($"[MainShip] Initialized. DeckProxy: {(deckProxy ? deckProxy.name : "NULL")}");
    }

    void LateUpdate()
    {
        foreach (var rider in riders)
        {
        
            Transform physics = rider.PhysicsRoot;
            Transform visuals = rider.VisualRoot;

            // Sync position
            Vector3 localPos = deckProxy.InverseTransformPoint(physics.position);
            visuals.position = transform.TransformPoint(localPos);
            
            Quaternion localRot = Quaternion.Inverse(deckProxy.rotation) * physics.rotation;
            visuals.rotation = transform.rotation * localRot;
            // var playerMovement = rider as PlayerMovement;
            // if (playerMovement != null && !playerMovement.isOwner)
            // {
            //     Quaternion localRot = Quaternion.Inverse(deckProxy.rotation) * physics.rotation;
            //     visuals.rotation = transform.rotation * localRot;
            // }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[MainShip OnTriggerEnter] Collider: {other.gameObject.name}");
        
        var rider = other.GetComponent<IShipProxyRider>();
        if (rider == null)
        {
            Debug.Log($"[MainShip] No rider component found");
            return;
        }

        if (riders.Add(rider))
        {
            Debug.Log($"[MainShip] ✓ Added rider. Count: {riders.Count}");
            EnterDeck(rider);
        }
    }

    // No OnTriggerExit here anymore!

    public void RemoveRider(IShipProxyRider rider)
    {
        if (riders.Remove(rider))
        {
            Debug.Log($"[MainShip] Removed rider. Count: {riders.Count}");
        }
    }

    void EnterDeck(IShipProxyRider rider)
    {
        Debug.Log($"[MainShip EnterDeck] START - Position: {rider.PhysicsRoot.position}");
        
        rider.OnEnterShipProxy(deckProxy, transform);

        // Get position relative to MainShip
        Vector3 localPos = transform.InverseTransformPoint(rider.PhysicsRoot.position);
        Quaternion localRot = Quaternion.Inverse(transform.rotation) * rider.PhysicsRoot.rotation;
        
        // Teleport to ProxyShip
        rider.PhysicsRoot.position = deckProxy.TransformPoint(localPos);
        rider.PhysicsRoot.rotation = deckProxy.rotation * localRot;
        
        Debug.Log($"[MainShip EnterDeck] Teleported to ProxyShip: {rider.PhysicsRoot.position}");

        Physics.IgnoreLayerCollision(
            LayerMask.NameToLayer("Player"),
            LayerMask.NameToLayer("Ship"),
            true
        );
        
        Debug.Log($"[MainShip EnterDeck] COMPLETE");
    }

    public void ExitDeck(IShipProxyRider rider)
    {
        Debug.Log($"[MainShip ExitDeck] START - Position: {rider.PhysicsRoot.position}");
        
        rider.OnExitShipProxy();
        
        // Transfer from ProxyShip back to MainShip
        Vector3 localPos = deckProxy.InverseTransformPoint(rider.PhysicsRoot.position);
        Quaternion localRot = Quaternion.Inverse(deckProxy.rotation) * rider.PhysicsRoot.rotation;
        
        rider.PhysicsRoot.position = transform.TransformPoint(localPos);
        rider.PhysicsRoot.rotation = transform.rotation * localRot;
        
        Debug.Log($"[MainShip ExitDeck] Teleported back to MainShip: {rider.PhysicsRoot.position}");

        Physics.IgnoreLayerCollision(
            LayerMask.NameToLayer("Player"),
            LayerMask.NameToLayer("Ship"),
            false
        );
        
        Debug.Log($"[MainShip ExitDeck] COMPLETE");
    }
}