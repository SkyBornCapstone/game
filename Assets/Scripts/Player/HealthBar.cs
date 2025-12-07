using System;
using UnityEngine;
using UnityEngine.UI;

namespace player
{
    public class HUDHealthBar : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        [SerializeField] private PlayerHealth playerHealth;

        public void Bind(PlayerHealth target)
        {
            if (target == null)
            {
                Debug.LogError("PlayerHealth Bind Error: Tried to bind to null player");
            }

            target.OnHealthChange += UpdateHealthBar;
            UpdateHealthBar(target.GetCurrentHealth() / target.maxHealth);

        }

        private void UpdateHealthBar(float normalizedHealth)
        {
            slider.value = normalizedHealth;
        }
    }
}