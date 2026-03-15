using PurrNet;
using UnityEngine;

namespace Player
{
    public class LocalPlayerVisibility : NetworkBehaviour
    {
        [SerializeField] private Renderer[] renderersToHide;

        protected override void OnOwnerChanged(PlayerID? oldOwner, PlayerID? newOwner, bool asServer)
        {
            if (isOwner)
            {
                Camera.main.cullingMask &= ~(1 << LayerMask.NameToLayer("PlayerBody"));
            }
        }
    }
}