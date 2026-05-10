using Damage;
using PurrNet;
using UnityEngine;

namespace Enemy
{
    public class EnemyHealth : NetworkBehaviour, IDamageable
    {
        [SerializeField] public float maxHealth = 100;
        [SerializeField] public SyncVar<float> health = new(0);

        private void Start()
        {
            health.value = maxHealth;
        }

        private void FixedUpdate()
        {
            if (!isServer) return;

            if (health.value <= 0)
            {
                Destroy(gameObject);
            }
        }

        public void TakeDamage(float damage)
        {
            ApplyDamage(damage);
        }

        [ServerRpc]
        public void ApplyDamage(float amount)
        {
            health.value -= amount;
        }
    }
}