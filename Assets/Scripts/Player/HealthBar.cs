using UnityEngine;
using UnityEngine.UI;
namespace player{
    public class HealthBarUI : MonoBehaviour
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

            playerHealth.onHealthChange += UpdateHealthBar;
        }

        private void UpdateHealthBar(float current, float max)
        {
            slider.maxValue = max;
            slider.value = current;
        }
    }
}

