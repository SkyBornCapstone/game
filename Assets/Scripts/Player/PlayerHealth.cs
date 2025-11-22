using System;
using PurrNet;
using PurrNet.Prediction;
using UnityEngine;
using UnityEngine.UI;

namespace player
{
    public class PlayerHealth : PredictedIdentity<PlayerHealth.HealthState>
    {
        [SerializeField] public float maxHealth = 100;
        [SerializeField] public float healthRegen = 0f;
        [SerializeField] public float healthRegenDelay = 0f;
        [SerializeField] public Slider healthSlider;

        [Header("Testing")] [SerializeField] private float testHealth = -1f; // -1 means use maxHealth

        public static Action<PlayerID?> OnDeath;

        protected override void Simulate(ref HealthState state, float delta)
        {
            // Handle regeneration
            state.timeSinceLastDamage += delta;
            if (state.timeSinceLastDamage >= healthRegenDelay && state.currentHealth > 0 &&
                state.currentHealth < maxHealth)
            {
                state.currentHealth = Mathf.Min(state.currentHealth + healthRegen * delta, maxHealth);
            }

            // Check for death
            if (state.currentHealth <= 0 && !state.isDead)
            {
                state.isDead = true;
                OnDeath?.Invoke(owner);

                predictionManager.hierarchy.Delete(gameObject);
            }
        }

        protected override HealthState GetInitialState()
        {
            return new HealthState
            {
                currentHealth = testHealth >= 0 ? testHealth : maxHealth,
                timeSinceLastDamage = healthRegenDelay,
                isDead = false
            };
        }

        protected override void UpdateView(HealthState healthState, HealthState? verified)
        {
            if (healthSlider)
            {
                healthSlider.value = healthState.currentHealth / maxHealth;
            }
        }

        // Public API for other systems
        public void TakeDamage(float amount)
        {
            currentState.currentHealth -= amount;
            currentState.timeSinceLastDamage = 0;

            if (currentState is { currentHealth: <= 0, isDead: false })
            {
                currentState.isDead = true;
                OnDeath?.Invoke(owner);

                predictionManager.hierarchy.Delete(gameObject);
            }
        }

        public void Heal(float amount)
        {
            currentState.currentHealth += amount;
            currentState.currentHealth = Mathf.Clamp(currentState.currentHealth, 0f, maxHealth);
        }

        public float GetCurrentHealth()
        {
            return currentState.currentHealth;
        }

        public bool IsDead()
        {
            return currentState.isDead;
        }

        // Testing method to set health directly
        public void SetHealthForTesting(float health)
        {
            if (!isOwner) return;
            currentState.currentHealth = Mathf.Clamp(health, 0f, maxHealth);
            currentState.isDead = currentState.currentHealth <= 0;
        }

        public struct HealthState : IPredictedData<HealthState>
        {
            public float currentHealth;
            public float timeSinceLastDamage;
            public bool isDead;

            public void Dispose()
            {
            }
        }
    }
}