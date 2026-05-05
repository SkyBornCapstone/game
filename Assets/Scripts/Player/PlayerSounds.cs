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
        private float _lastHitSoundTime;
        private float _lastSwingSoundTime = -1f;

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
            _lastSwingSoundTime = Time.time;
            _audioSource.PlayOneShot(swordSwingSound);
        }

        [ObserversRpc(runLocally: true)]
        public void PlaySwordHit()
        {
            if (_audioSource == null || swordHitSound == null) return;
            
            if (Time.time - _lastSwingSoundTime > 0.5f) return;
            if (Time.time - _lastHitSoundTime < 0.5f) return;
            
            _lastHitSoundTime = Time.time;
            _audioSource.PlayOneShot(swordHitSound);
        }
    }
}
