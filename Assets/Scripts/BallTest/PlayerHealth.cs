using System;
using PurrNet;
using PurrNet.Prediction;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using balltest;
namespace balltest
{
    public class PlayerHealth : PredictedIdentity<PlayerHealth.State>
    {
        [SerializeField] private Slider healthSlider;
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private int damage = 10;
        [SerializeField] private ParticleSystem deathParticles;

        public static Action<PlayerID?> OnDeathAction;
        public static Action ClearPlayers;
    
        private PredictedEvent _onDeath;

        protected override void LateAwake()
        {
            _onDeath = new PredictedEvent(predictionManager, this);
            _onDeath.AddListener(OnDeath);
            ClearPlayers += OnClearPlayers;
        }

        protected override void OnDestroy()
        {
            _onDeath.RemoveListener(OnDeath);
            ClearPlayers -= OnClearPlayers;
        }

        private void OnClearPlayers()
        {
            predictionManager.hierarchy.Delete(gameObject);
        }

        private void OnDeath()
        {
            Instantiate(deathParticles, transform.position, Quaternion.identity);
        }

        protected override State GetInitialState()
        {
            return new State
            {
                Health = maxHealth
            };
        }

        public void HitOtherPlayer()
        {
            TakeDamage(damage);
        }

        public void TakeDamage(int amount)
        {
            currentState.Health -= amount;
        
            if (currentState is { Health: <= 0, IsDead: false })
            {
                currentState.IsDead = true;
                _onDeath?.Invoke();
                OnDeathAction.Invoke(owner);
            
                predictionManager.hierarchy.Delete(gameObject);
            }
        }
    

        protected override void UpdateView(State viewState, State? verified)
        {
            base.UpdateView(viewState, verified);

            if (healthSlider)
            {
                healthSlider.value = viewState.Health / (float) maxHealth;
            }
        }
    
        public struct State : IPredictedData<State>
        {
            public int Health;
            public bool IsDead;
        
            public void Dispose() { }
        }
    }
}

