using UnityEngine;
using UnityEngine.UI;

namespace player
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        [SerializeField] private PlayerHealth playerHealth;

        void Start()
        {
            if (playerHealth == null)
            {
                Debug.LogError("HealthBarUI missing playerHealth reference!");
                return;
            }

            // Subscribe to health changes
            playerHealth.onHealthChange += UpdateHealthBar;

            // Initialize the health bar immediately
            UpdateHealthBar(playerHealth.GetCurrentHealth(), playerHealth.maxHealth);
        }

        private void UpdateHealthBar(float current, float max)
        {
            if (slider == null) return;

            slider.maxValue = max;
            slider.value = current;
        }

        private void OnDestroy()
        {
            // Unsubscribe to prevent memory leaks
            if (playerHealth != null)
            {
                playerHealth.onHealthChange -= UpdateHealthBar;
            }
        }
    }
}