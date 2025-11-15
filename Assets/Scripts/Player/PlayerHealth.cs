using System;
using PurrNet;
using PurrNet.Prediction;
using UnityEngine;

namespace player
{
    public class PlayerHealth : PredictedIdentity<PlayerHealth.HealthInput, PlayerHealth.HealthState>
    {
        [SerializeField] public float maxHealth = 100;
        [SerializeField] public float healthRegen = 0f;
        [SerializeField] public float healthRegenDelay = 0f;
        
        [Header("Testing")]
        [SerializeField] private float testHealth = -1f; // -1 means use maxHealth

        public System.Action<float, float> onHealthChange;
        public System.Action onDeath;

        HealthState currentState;
        private float queuedDamage = 0f;
        private float queuedHealing = 0f;

        protected override void LateAwake()
        {
            // Initialize health to max or test value
            currentState.currentHealth = testHealth >= 0 ? testHealth : maxHealth;
            currentState.timeSinceLastDamage = healthRegenDelay;
            currentState.isDead = false;
            
            // Trigger initial health update for UI
            onHealthChange?.Invoke(currentState.currentHealth, maxHealth);
        }
        
        protected override void Simulate(HealthInput input, ref HealthState state, float delta)
        {
            // Initialize state on first run
            if (state.currentHealth == 0 && !state.isDead)
            {
                state.currentHealth = testHealth >= 0 ? testHealth : maxHealth;
                state.timeSinceLastDamage = healthRegenDelay;
            }

            // Apply damage
            if (input.damageAmount > 0)
            {
                state.currentHealth -= input.damageAmount;
                state.timeSinceLastDamage = 0f;
                input.damageAmount = 0f;
            }
            
            // Apply healing
            if (input.healAmount > 0)
            {
                state.currentHealth += input.healAmount;
            }

            // Clamp health
            state.currentHealth = Mathf.Clamp(state.currentHealth, 0f, maxHealth);

            // Handle regeneration
            state.timeSinceLastDamage += delta;
            if (state.timeSinceLastDamage >= healthRegenDelay && state.currentHealth > 0 && state.currentHealth < maxHealth)
            {
                state.currentHealth = Mathf.Min(state.currentHealth + healthRegen * delta, maxHealth);
            }
            
            // Check for death
            if (state.currentHealth <= 0 && !state.isDead)
            {
                state.isDead = true;
            }

            // Store current state for external access
            currentState = state;
        }
        
        protected override void UpdateView(HealthState healthState, HealthState? verified)
        {
            onHealthChange?.Invoke(healthState.currentHealth, maxHealth);
            
            if (healthState.isDead)
            {
                onDeath?.Invoke();
            }
        }
        
        protected override void UpdateInput(ref HealthInput input)
        {
            // Input is handled in GetFinalInput
        }

        protected override void GetFinalInput(ref HealthInput input)
        {
            // Apply queued damage and healing
            input.damageAmount = queuedDamage;
            input.healAmount = queuedHealing;
            
            // Clear queued values after applying
            queuedDamage = 0f;
            queuedHealing = 0f;
        }

        // Public API for other systems
        public void TakeDamage(float amount)
        {
            if (!isOwner) return;
            queuedDamage += amount;
        }
        
        public void Heal(float amount)
        {
            if (!isOwner) return;
            queuedHealing += amount;
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

            public void Dispose() { }
        }

        public struct HealthInput : IPredictedData
        {
            public float damageAmount;
            public float healAmount;

            public void Dispose() { }
        }
    }
}