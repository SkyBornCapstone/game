using UnityEngine;
using Ship;
public class ProxyShipExitTrigger : MonoBehaviour
{
    [Header("References")]
    public MainShipDeckTriggerEnter mainShipBridge;

    void Start()
    {
        Debug.Log($"[ProxyShip] Exit trigger initialized");
    }

    void OnTriggerExit(Collider other)
    {
        Debug.Log($"[ProxyShip OnTriggerExit] Collider: {other.gameObject.name}");
        
        var rider = other.GetComponent<IShipProxyRider>();
        if (rider == null)
        {
            Debug.Log($"[ProxyShip] No rider component found");
            return;
        }

        Debug.Log($"[ProxyShip] Player left ProxyShip, teleporting back to MainShip");
        
        // Tell the main ship bridge to handle the exit
        mainShipBridge.RemoveRider(rider);
        mainShipBridge.ExitDeck(rider);
    }
}
