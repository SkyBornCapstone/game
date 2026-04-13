using UnityEngine;

public class SwordAnimationEventHandler : MonoBehaviour
{
    [SerializeField] private GameObject swordPrefab;
    [SerializeField] private GameObject swordDamage;
    
    private Collider[] colliders;
    private Collider triggerCollider;
    private Collider solidCollider;

    private void Awake()
    {
        colliders = swordDamage.GetComponents<Collider>();
        
        foreach (var col in colliders)
        {
            if (col.isTrigger)
                triggerCollider = col;
            else
                solidCollider = col;
        }
    }
    public void ShowSword()
    {
        swordPrefab.SetActive(true);
    }

    public void HideSword()
    {
        swordPrefab.SetActive(false);
    }

    public void ActivateSwordCollider()
    {
        if (solidCollider != null)  solidCollider.enabled  = true;
    }

    public void DeactivateSwordCollider()
    {
        if (solidCollider != null)  solidCollider.enabled  = false;
        
    }
}
