using UnityEngine;
using UnityEngine.UI;

namespace player
{
    public class HUDHealthBar : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        [SerializeField] private PlayerHealth playerHealth;

        private PlayerHealth _health;

        public void Bind(PlayerHealth target)
        {
            if (target == null)
            {
                Debug.LogError("PlayerHealth Bind Error: Tried to bind to null player");
            }

            _health = target;
            target.health.onChanged += UpdateHealthBar;
            UpdateHealthBar(target.GetHealth());
        }

        private void UpdateHealthBar(float newHealth)
        {
            float normalizedHealth = newHealth / _health.maxHealth;
            slider.value = normalizedHealth;
        }
    }
}