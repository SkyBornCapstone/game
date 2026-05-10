using System;
using PurrNet;
using UnityEngine;

namespace player
{
    public class PlayerHealth : NetworkBehaviour
    {
        [SerializeField] public float maxHealth = 100;
        [SerializeField] public float healthRegen = 0f;
        [SerializeField] public float healthRegenDelay = 0f;
        [SerializeField] public SyncVar<float> health = new(100);

        private float _timeSinceLastDamage = 0;

        public static Action<PlayerID?> OnDeath;

        private void FixedUpdate()
        {
            if (!isServer) return;

            // Handle regeneration
            _timeSinceLastDamage += Time.fixedDeltaTime;
            if (_timeSinceLastDamage >= healthRegenDelay && health.value > 0 &&
                health.value < maxHealth)
            {
                health.value = Mathf.Min(health.value + healthRegen * Time.deltaTime, maxHealth);
            }

            if (transform.position.y < -100)
            {
                TakeDamage(5f);
            }

            // Check for death
            if (health.value <= 0)
            {
                OnDeath?.Invoke(owner);

                Destroy(gameObject.transform.parent.gameObject);
            }
        }

        protected override void OnSpawned()
        {
            if (!isOwner) return;

            var hud = FindFirstObjectByType<HUDHealthBar>();
            if (hud)
                hud.Bind(this);
            else
                Debug.LogError("PlayerHealth: HUDHealthBar not found");
        }

        [ServerRpc]
        public void TakeDamage(float amount)
        {
            health.value -= amount;
            _timeSinceLastDamage = 0;
            RpcOnTookDamage();
        }

        [ObserversRpc(runLocally: true)]
        private void RpcOnTookDamage()
        {
            if (isOwner && MusicController.Instance != null)
            {
                MusicController.Instance.OnPlayerTookDamage();
            }
        }

        [ServerRpc]
        public void Heal(float amount)
        {
            health.value += amount;
            health.value = Mathf.Clamp(health.value, 0f, maxHealth);
        }

        public float GetHealth()
        {
            return health.value;
        }
    }
}