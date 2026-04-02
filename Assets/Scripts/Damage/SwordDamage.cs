using player;
using PurrNet;
using UnityEngine;

namespace Damage
{
    public class SwordDamage : NetworkBehaviour
    {
        [SerializeField] private int damage = 20;
        // [SerializeField] private bool destroyOnHit = true;
        // [SerializeField] private Rigidbody rb;

        private void OnTriggerEnter(Collider other)
        {
            
            if (other.gameObject.TryGetComponent(out PlayerHealth playerHealth))
            {
                // if (playerHealth.isOwner) return;
                playerHealth.TakeDamage(damage);
            }
            
        }
    }
}