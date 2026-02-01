using PurrNet;
using UnityEngine;

namespace Ship.ShipControllers
{
    public class LiftControl : ShipControlStation
    {
        [Header("References")] [SerializeField]
        private ShipControllerV2 ship;

        [SerializeField] private ParticleSystem thrusterParticles;

        [Header("Lift Settings")] [SerializeField]
        private int maxLevel = 2;

        [SerializeField] private int minLevel = -2;
        [SerializeField] private float level1Speed = .25f; // Speed at level ±1
        [SerializeField] private float level2Speed = .75f; // Speed at level ±2

        [Header("Visual Feedback")] [SerializeField]
        private float particleEmissionMultiplier = 10f;

        public SyncVar<int> _currentLevel = new();

        protected override void OnSpawned()
        {
            _currentLevel.onChanged += UpdateVisuals;
        }

        protected override void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                ChangeLiftLevel(1);
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                ChangeLiftLevel(-1);
            }
        }

        [ServerRpc]
        private void ChangeLiftLevel(int levelChange)
        {
            _currentLevel.value += levelChange;
            _currentLevel.value = Mathf.Clamp(_currentLevel.value, minLevel, maxLevel);

            float liftSpeed = GetLiftSpeedForLevel(_currentLevel.value);
            ship.SetLiftThrottle(liftSpeed);
        }

        private float GetLiftSpeedForLevel(int level)
        {
            return level switch
            {
                2 => level2Speed,
                1 => level1Speed,
                0 => 0f,
                -1 => -level1Speed,
                -2 => -level2Speed,
                _ => 0f
            };
        }

        private void UpdateVisuals(int newLevel)
        {
            if (!thrusterParticles)
                return;

            var emission = thrusterParticles.emission;

            // Only show particles when ascending (positive levels)
            if (_currentLevel.value >= 0)
            {
                if (!thrusterParticles.isPlaying)
                    thrusterParticles.Play();

                var main = thrusterParticles.main;
                main.startLifetime = 2 + _currentLevel.value;
                emission.rateOverTime = (_currentLevel.value + 1) * particleEmissionMultiplier;
            }
            else
            {
                if (thrusterParticles.isPlaying)
                    thrusterParticles.Stop();
            }
        }
    }
}