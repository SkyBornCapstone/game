using System.Collections.Generic;
using Player;
using PurrNet;
using UnityEngine;

namespace Ship
{
    public class MainShipDeckTriggerEnter : NetworkBehaviour
    {
        [Header("References")] public Transform deckProxy; // ProxyShip transform
        public Transform visualShipRoot; // The Main Ship Root (Visual world)

        private readonly HashSet<IShipProxyRider> _riders = new();

        void LateUpdate()
        {
            foreach (var rider in _riders)
            {
                Transform physics = rider.PhysicsRoot;
                Transform visuals = rider.VisualRoot;

                // Sync position
                Vector3 localPos = deckProxy.InverseTransformPoint(physics.position);
                visuals.position = visualShipRoot.TransformPoint(localPos);

                Quaternion localRot = Quaternion.Inverse(deckProxy.rotation) * physics.rotation;
                visuals.rotation = visualShipRoot.rotation * localRot;
            }
        }

        void OnTriggerEnter(Collider other)
        {
            var rider = other.GetComponent<IShipProxyRider>();
            if (rider == null)
            {
                return;
            }

            var netRider = other.GetComponent<NetworkIdentity>();
            if (netRider == null || !netRider.isController)
            {
                return;
            }

            EnterDeck(rider);
        }

        void EnterDeck(IShipProxyRider rider)
        {
            AddRider(rider);

            rider.OnEnterShipProxy(deckProxy, visualShipRoot);

            // Get position relative to MainShip
            Vector3 localPos = visualShipRoot.InverseTransformPoint(rider.PhysicsRoot.position);
            Quaternion localRot = Quaternion.Inverse(visualShipRoot.rotation) * rider.PhysicsRoot.rotation;

            // Teleport to ProxyShip
            rider.PhysicsRoot.position = deckProxy.TransformPoint(localPos);
            rider.PhysicsRoot.rotation = deckProxy.rotation * localRot;

            rider.PhysicsRoot.GetComponent<FirstPersonCamera>().SetShipContext(visualShipRoot, deckProxy);
        }

        [ObserversRpc(runLocally: true, requireServer: false)]
        void AddRider(IShipProxyRider rider)
        {
            _riders.Add(rider);
        }

        public void ExitDeck(IShipProxyRider rider)
        {
            RemoveRider(rider);

            rider.OnExitShipProxy();

            // Transfer from ProxyShip back to MainShip
            Vector3 localPos = deckProxy.InverseTransformPoint(rider.PhysicsRoot.position);
            Quaternion localRot = Quaternion.Inverse(deckProxy.rotation) * rider.PhysicsRoot.rotation;

            rider.PhysicsRoot.position = visualShipRoot.TransformPoint(localPos);
            rider.PhysicsRoot.rotation = visualShipRoot.rotation * localRot;

            rider.PhysicsRoot.GetComponent<FirstPersonCamera>().ClearShipContext();
        }

        [ObserversRpc(runLocally: true, requireServer: false)]
        void RemoveRider(IShipProxyRider rider)
        {
            _riders.Remove(rider);
        }
    }
}