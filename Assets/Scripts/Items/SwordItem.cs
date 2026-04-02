using Interaction;
using player;
using UnityEngine;

namespace Items
{
    [RequireComponent(typeof(AudioSource))]
    public class SwordItem : Grabbable
    {
        [SerializeField] private AudioClip swingSound;
        [SerializeField] private AudioClip hitSound;

        private AudioSource _audioSource;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
        }
        public override void Use()
        {
            Debug.Log("Get sworded");
            if (_audioSource != null && swingSound != null)
            {
                _audioSource.PlayOneShot(swingSound);
            }
        }

        private void OnCollisionEnter(Collision other)
        {
            if (!isOwner) return;

            if (other.gameObject.TryGetComponent<PlayerHealth>(out var health))
            {
                health.TakeDamage(10);
                if (_audioSource != null && hitSound != null)
                {
                    _audioSource.PlayOneShot(hitSound);
                }
            }
        }
    }
}