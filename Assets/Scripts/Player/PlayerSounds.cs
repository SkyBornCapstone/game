using UnityEngine;
using PurrNet;

namespace Player
{
    [RequireComponent(typeof(AudioSource))]
    public class PlayerSounds : NetworkBehaviour
    {
        private AudioSource _audioSource;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        [ObserversRpc(runLocally: true)]
        public void PlaySwordUnsheathe()
        {
            if (_audioSource == null) return;
            // TODO: Play sword unsheathe sound
        }

        [ObserversRpc(runLocally: true)]
        public void PlaySwordSheathe()
        {
            if (_audioSource == null) return;
            // TODO: Play sword sheathe sound
        }

        [ObserversRpc(runLocally: true)]
        public void PlaySwordSwing()
        {
            if (_audioSource == null) return;
            // TODO: Play sword swing sound
        }

        [ObserversRpc(runLocally: true)]
        public void PlaySwordHit()
        {
            if (_audioSource == null) return;
            // TODO: Play sword hit sound
        }
    }
}
