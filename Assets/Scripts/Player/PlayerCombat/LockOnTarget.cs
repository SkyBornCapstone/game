using UnityEngine;

namespace Player.PlayerCombat
{
    public class LockOnTarget : MonoBehaviour
    {
        public Transform aimPoint;
        private bool canBeLocked = true;
        public bool CanBeLocked => canBeLocked;

        public void SetLockable(bool value)
        {
            canBeLocked = value;
        }

    }
}