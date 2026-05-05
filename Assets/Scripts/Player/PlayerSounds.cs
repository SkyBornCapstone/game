using UnityEngine;
using PurrNet;

namespace Player
{
    [RequireComponent(typeof(AudioSource))]
    public class PlayerSounds : NetworkBehaviour
    {
        [Header("Sword Sounds")]
        [SerializeField] private AudioClip swordUnsheatheSound;
        [SerializeField] private AudioClip swordSheatheSound;
        [SerializeField] private AudioClip swordSwingSound;
        [SerializeField] private AudioClip swordHitSound;

        private AudioSource _audioSource;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        [ObserversRpc(runLocally: true)]
        public void PlaySwordUnsheathe()
        {
            if (_audioSource == null || swordUnsheatheSound == null) return;
            _audioSource.PlayOneShot(swordUnsheatheSound);
        }

        [ObserversRpc(runLocally: true)]
        public void PlaySwordSheathe()
        {
            if (_audioSource == null || swordSheatheSound == null) return;
            _audioSource.PlayOneShot(swordSheatheSound);
        }

        [ObserversRpc(runLocally: true)]
        public void PlaySwordSwing()
        {
            if (_audioSource == null || swordSwingSound == null) return;
            _audioSource.PlayOneShot(swordSwingSound);
        }

        [ObserversRpc(runLocally: true)]
        public void PlaySwordHit()
        {
            if (_audioSource == null || swordHitSound == null) return;
            _audioSource.PlayOneShot(swordHitSound);
        }
    }
}
