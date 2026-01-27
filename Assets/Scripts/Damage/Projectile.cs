using player;
using PurrNet;
using UnityEngine;

namespace Damage
{
    public class Projectile : NetworkBehaviour
    {
        [SerializeField] private int damage = 20;
        [SerializeField] private bool destroyOnHit = true;
        [SerializeField] private Rigidbody rb;

        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.TryGetComponent(out PlayerHealth playerHealth))
            {
                playerHealth.TakeDamage(damage);
            }

            if (destroyOnHit)
            {
                Destroy(gameObject);
            }
        }
    }
}