using Player.PlayerCombat;
using UnityEngine;

public class Punch : MonoBehaviour
{
    [SerializeField] private CombatControllerv2 ownerCombat;
    [SerializeField] private float knockback = 1;
    private void OnTriggerEnter(Collider other)
    {

        if (other.TryGetComponent(out CombatControllerv2 hitCombat) && hitCombat == ownerCombat)
        {
            print("Own Collider");
            return;
        }
        
        if (other.TryGetComponent(out CombatControllerv2 otherCombat))
        {
            print("PUNCH");
            if(otherCombat.isBlocking == true)
                otherCombat.handleStun();
            else
            {
                print("no block");
            }
            ApplyKnockback(other);
            return;
        }
        
    }
    private void ApplyKnockback(Collider other)
    {
        if (other.TryGetComponent(out Rigidbody rb))
        {
            Vector3 knockbackDir = -other.transform.forward;
            rb.AddForce(knockbackDir * knockback, ForceMode.VelocityChange);
        }
    }


}
