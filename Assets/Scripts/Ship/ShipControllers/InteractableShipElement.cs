using PurrNet;

namespace Ship.ShipControllers
{
    public abstract class InteractableShipElement : NetworkBehaviour
    {
        public ShipInteractType shipInteractType;
    }
}