using UnityEngine;



namespace Ship
{
    public interface IShipProxyRider
    {
        Transform PhysicsRoot { get; }
        Transform VisualRoot { get; }

        void OnEnterShipProxy(Transform proxy, Transform realShip);
        void OnExitShipProxy();
    }
}