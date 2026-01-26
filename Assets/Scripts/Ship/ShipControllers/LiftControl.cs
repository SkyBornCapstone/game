using Player;
using UnityEngine;

namespace Ship.ShipControllers
{
    public class LiftControl : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ShipControllerV2 ship;
        [SerializeField] private ParticleSystem thrusterParticles;
        
        [Header("Lift Settings")]
        [SerializeField] private int maxLevel = 2;
        [SerializeField] private int minLevel = -2;
        [SerializeField] private float level1Speed = .25f;   // Speed at level ±1
        [SerializeField] private float level2Speed = .75f;   // Speed at level ±2
        [SerializeField] private float inputCooldown = 0.3f;
        [Header("Visual Feedback")]
        [SerializeField] private float particleEmissionMultiplier = 10f;
        
        public int CurrentLevel => currentLevel;
        
        private int currentLevel = 0;
        private PlayerMovement currentPlayer;
        private float lastInputTime;
        void Update()
        {
            if (!currentPlayer || !ship)
                return;
            print(currentLevel);
            HandleInput();
            UpdateShipLift();
            UpdateVisuals();
        }

        private void HandleInput()
        {
            if (Time.time - lastInputTime < inputCooldown)
                return;
            float input = ShipInputManager.Instance.GetControlInput();

            if (input > 0)
            {
                IncreaseLevel();
                lastInputTime = Time.time;
            }
            else if (input < 0)
            {
                DecreaseLevel();
                lastInputTime = Time.time;
            }

        }

        private void IncreaseLevel()
        {
            currentLevel++;
            currentLevel = Mathf.Clamp(currentLevel, minLevel, maxLevel);
        }

        private void DecreaseLevel()
        {
            currentLevel--;
            currentLevel = Mathf.Clamp(currentLevel, minLevel, maxLevel);
        }

        private void UpdateShipLift()
        {
            float liftSpeed = GetLiftSpeedForLevel(currentLevel);
            ship.SetLiftThrottle(liftSpeed);
        }

        private float GetLiftSpeedForLevel(int level)
        {
            switch (level)
            {
                case 2:
                    return level2Speed;
                case 1:
                    return level1Speed;
                case 0:
                    return 0f;
                case -1:
                    return -level1Speed;
                case -2:
                    return -level2Speed;
                default:
                    return 0f;
            }
        }

        private void UpdateVisuals()
        {
            if (!thrusterParticles)
                return;
                
            var emission = thrusterParticles.emission;
            
            // Only show particles when ascending (positive levels)
            if (currentLevel > 0)
            {
                if (!thrusterParticles.isPlaying)
                    thrusterParticles.Play();
                    
                emission.rateOverTime = currentLevel * particleEmissionMultiplier;
            }
            else
            {
                if (thrusterParticles.isPlaying)
                    thrusterParticles.Stop();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out PlayerMovement player))
                currentPlayer = player;
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out PlayerMovement player) && player == currentPlayer)
            {
                currentPlayer = null;
            }
        }
    }
}