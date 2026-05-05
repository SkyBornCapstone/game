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
        public async void PlaySwordUnsheathe()
        {
            if (_audioSource == null || swordUnsheatheSound == null) return;
            
            await System.Threading.Tasks.Task.Delay(100);
            
            if (this != null && _audioSource != null && swordUnsheatheSound != null)
                _audioSource.PlayOneShot(swordUnsheatheSound);
        }

        [ObserversRpc(runLocally: true)]
        public async void PlaySwordSheathe()
        {
            if (_audioSource == null || swordSheatheSound == null) return;
            
            await System.Threading.Tasks.Task.Delay(250);
            
            if (this != null && _audioSource != null && swordSheatheSound != null)
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
