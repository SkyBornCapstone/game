using System;
using PurrNet;
using UnityEngine;
using UnityEngine.UI;

namespace BallTest
{
    public class PlayerHealth : NetworkIdentity
    {
        [SerializeField] private Slider healthSlider;
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private int damage = 10;
        [SerializeField] private ParticleSystem deathParticles;

        public static event Action<PlayerID?> OnDeath;

        [SerializeField] private SyncVar<int> health = new(100);

        protected override void OnSpawned(bool asServer)
        {
            health.value = maxHealth;
            health.onChanged += OnHealthChanged;
        }

        private void OnHealthChanged(int newHealth)
        {
            healthSlider.value = newHealth / (float)maxHealth;
        }

        public void HitOtherPlayer()
        {
            TakeDamage(damage);
        }

        [ServerRpc]
        public void TakeDamage(int amount)
        {
            health.value -= amount;

            if (health.value <= 0)
            {
                Destroy(gameObject);
                OnDeath?.Invoke(owner);
            }
        }

        protected override void OnDestroy()
        {
            if (health.value == 0)
            {
                Instantiate(deathParticles, transform.position, Quaternion.identity);
            }
        }
    }
}