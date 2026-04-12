using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SwordVisibilityHandler : MonoBehaviour
{
    [SerializeField] private GameObject swordPrefab;
    [SerializeField] private AudioClip drawSound;
    [SerializeField] private AudioClip sheathSound;

    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void ShowSword()
    {
        swordPrefab.SetActive(true);
        if (_audioSource != null && drawSound != null)
        {
            _audioSource.PlayOneShot(drawSound);
        }
    }

    public void HideSword()
    {
        swordPrefab.SetActive(false);
        if (_audioSource != null && sheathSound != null)
        {
            _audioSource.PlayOneShot(sheathSound);
        }
    }
}
