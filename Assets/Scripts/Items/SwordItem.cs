using Interaction;
using player;
using UnityEngine;

namespace Items
{
    public class SwordItem : Grabbable
    {
        public override void Use()
        {
            Debug.Log("Get sworded");
        }

        private void OnCollisionEnter(Collision other)
        {
            if (!isOwner) return;

            if (other.gameObject.TryGetComponent<PlayerHealth>(out var health))
            {
                health.TakeDamage(10);
            }
        }
    }
}