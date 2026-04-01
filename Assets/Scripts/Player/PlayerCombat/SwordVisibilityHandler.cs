using UnityEngine;

public class SwordVisibilityHandler : MonoBehaviour
{
    [SerializeField] private GameObject swordPrefab;

    public void ShowSword()
    {
        swordPrefab.SetActive(true);
    }

    public void HideSword()
    {
        swordPrefab.SetActive(false);
    }
}
