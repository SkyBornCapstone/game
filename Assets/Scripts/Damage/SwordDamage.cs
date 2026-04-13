using player;
using Player.PlayerCombat;
using PurrNet;
using UnityEngine;

using Player.PlayerCombat;
using PurrNet;
using UnityEngine;

namespace Damage
{
    public class SwordDamage : NetworkBehaviour
    {
        [SerializeField] private int damage = 20;
        [SerializeField] private CombatControllerv2 ownerCombat; 

        private void OnTriggerEnter(Collider other)
        {

            if (other.TryGetComponent(out CombatControllerv2 hitCombat) && hitCombat == ownerCombat)
                return;

            if (other.TryGetComponent(out SwordCollider sword) && sword.ownerCombat != ownerCombat)
            {
                if (sword.ownerCombat.isBlocking) return;
            }

            if (other.TryGetComponent(out PlayerHealth playerHealth))
            {
                if (other.TryGetComponent(out CombatControllerv2 combat) && combat.isBlocking)
                    playerHealth.TakeDamage(damage / 2);
                else
                    playerHealth.TakeDamage(damage);
            }
        }
        
        // private void OnCollisionEnter(Collision other)
        // {
        //     if (other.gameObject.TryGetComponent(out SwordCollider enemySword))
        //     {
        //         if (enemySword.ownerCombat == ownerCombat) return;
        //         
        //     }
        // }
    }
}