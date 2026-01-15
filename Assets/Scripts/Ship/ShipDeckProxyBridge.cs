using UnityEngine;
using Player;

public class ShipDeckProxyBridge : MonoBehaviour
{
    [Header("References")]
    public Transform deckProxy;        // Static deck collider
    public Transform playerPhysics;    // Player PhysicsRoot
    public Transform playerVisual;     // Player VisualRoot
    public PlayerMovement playerMovement;

    private bool onDeck;
  

    void LateUpdate()
    {
        if (!onDeck) return;

        // Keep visuals riding the ship
        Vector3 localPos =
            deckProxy.InverseTransformPoint(playerPhysics.position);

        playerVisual.position =
            transform.TransformPoint(localPos);

        playerVisual.rotation = transform.rotation;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        EnterDeck();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        ExitDeck();
    }

    void EnterDeck()
    {
        onDeck = true;
        playerMovement.isOnShipDeck = true;

        // Move physics onto static deck
        Vector3 localPos =
            transform.InverseTransformPoint(playerPhysics.position);

        playerPhysics.position =
            deckProxy.TransformPoint(localPos);

        Physics.IgnoreLayerCollision(
            LayerMask.NameToLayer("Player"),
            LayerMask.NameToLayer("Ship"),
            true
        );
    }

    void ExitDeck()
    {
        onDeck = false;
        playerMovement.isOnShipDeck = false;

        // Move physics back to world
        Vector3 localPos =
            deckProxy.InverseTransformPoint(playerPhysics.position);

        playerPhysics.position =
            transform.TransformPoint(localPos);

        Physics.IgnoreLayerCollision(
            LayerMask.NameToLayer("Player"),
            LayerMask.NameToLayer("Ship"),
            false
        );
    }
}