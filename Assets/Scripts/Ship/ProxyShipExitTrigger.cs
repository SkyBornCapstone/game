using PurrNet;
using UnityEngine;

namespace Ship
{
    public class ProxyShipExitTrigger : NetworkBehaviour
    {
        [Header("References")] public MainShipDeckTriggerEnter mainShipBridge;

        void OnTriggerExit(Collider other)
        {
            var rider = other.GetComponent<IShipProxyRider>();
            if (rider == null) return;
            var netRider = other.GetComponent<NetworkIdentity>();
            if (netRider == null || !netRider.isController) return;

            // Tell the main ship bridge to handle the exit
            mainShipBridge.ExitDeck(rider);
        }
    }
}