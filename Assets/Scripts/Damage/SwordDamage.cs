using player;
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
            {
                print("Own Collider");
                return;
            }
            
            if (ownerCombat != null && ownerCombat.TryGetComponent(out Player.PlayerSounds playerSounds))
            {
                playerSounds.PlaySwordHit();
            }
                
            if (other.TryGetComponent(out SwordCollider sword))
            {
                print("GOT SWORD COLLIDER");
                if (sword.ownerCombat.isBlocking) return;
                return;
            }
        
            if (other.TryGetComponent(out PlayerHealth playerHealth))
            {
                print("Got player health");
                print(hitCombat.isBlocking);
                if (other.TryGetComponent(out CombatControllerv2 combat) && combat.isBlocking)
                {
                    print("TAKING HALF DAMAGE");
                    playerHealth.TakeDamage(damage / 5);
                }
                else
                    playerHealth.TakeDamage(damage);
            }
        }
        
        // private void OnCollisionEnter(Collision other)
        // {
        //     if (other.gameObject.TryGetComponent(out SwordCollider enemySword))
        //     {
        //         print("Hit Sword Collider");
        //         // if (enemySword.ownerCombat == ownerCombat) return;
        //         
        //     }
        // }
    }
}