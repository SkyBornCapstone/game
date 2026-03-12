using PurrNet;
using UnityEngine;

public class LocalPlayerVisibility : NetworkBehaviour
{
    [SerializeField] private Renderer[] renderersToHide;

    protected override void OnSpawned()
    {
        if (isOwner)
        {
            Camera.main.cullingMask &= ~(1 << LayerMask.NameToLayer("PlayerBody"));
        }
    }
}