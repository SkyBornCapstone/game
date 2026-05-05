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

        [Header("Footstep Sounds")]
        [SerializeField] private AudioClip[] woodFootstepSounds;
        [SerializeField] private AudioClip[] grassFootstepSounds;
        [SerializeField] private float baseStepDistance = 1.5f;
        [SerializeField] private float minimumSpeedToStep = 0.5f;
        [SerializeField] [Range(0f, 1f)] private float footstepVolume = 0.3f;

        private AudioSource _audioSource;
        private float _lastHitSoundTime;
        private float _lastSwingSoundTime = -1f;
        private Vector3 _lastPosition;
        private float _distanceAccumulator;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        private void Start()
        {
            _lastPosition = transform.position;
        }

        private void Update()
        {
            Vector3 currentPosition = transform.position;
            // Calculate distance moved on the XZ plane (ignore vertical movement like jumping/falling)
            float distanceMoved = Vector3.Distance(new Vector3(currentPosition.x, 0, currentPosition.z), new Vector3(_lastPosition.x, 0, _lastPosition.z));
            
            // Only count distance if moving faster than the threshold
            float speed = distanceMoved / Time.deltaTime;
            if (speed > minimumSpeedToStep)
            {
                _distanceAccumulator += distanceMoved;

                if (_distanceAccumulator >= baseStepDistance)
                {
                    PlayFootstepSound();
                    _distanceAccumulator -= baseStepDistance; // Keep remainder for smooth continuous movement
                }
            }
            else
            {
                _distanceAccumulator = 0f;
            }

            _lastPosition = currentPosition;
        }

        private void PlayFootstepSound()
        {
            AudioClip[] currentFootsteps = grassFootstepSounds; // Default to grass

            // Raycast down slightly from above the player's feet
            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out RaycastHit hit, 2.0f))
            {
                // Check if we hit the ship
                if (hit.collider.CompareTag("Ship") || hit.collider.name.ToLower().Contains("ship"))
                {
                    currentFootsteps = woodFootstepSounds;
                }
                else
                {
                    currentFootsteps = grassFootstepSounds;
                }
            }

            if (currentFootsteps == null || currentFootsteps.Length == 0) return;
            
            AudioClip clip = currentFootsteps[Random.Range(0, currentFootsteps.Length)];
            if (clip != null)
                _audioSource.PlayOneShot(clip, footstepVolume);
        }

        [ObserversRpc(runLocally: true)]
        public async void PlaySwordUnsheathe()
        {
            if (_audioSource == null || swordUnsheatheSound == null) return;
            
            if (isOwner && MusicController.Instance != null)
                MusicController.Instance.OnSwordUnsheathed();
            
            await System.Threading.Tasks.Task.Delay(100);
            
            if (this != null && _audioSource != null && swordUnsheatheSound != null)
                _audioSource.PlayOneShot(swordUnsheatheSound);
        }

        [ObserversRpc(runLocally: true)]
        public async void PlaySwordSheathe()
        {
            if (_audioSource == null || swordSheatheSound == null) return;
            
            if (isOwner && MusicController.Instance != null)
                MusicController.Instance.OnSwordSheathed();
            
            await System.Threading.Tasks.Task.Delay(250);
            
            if (this != null && _audioSource != null && swordSheatheSound != null)
                _audioSource.PlayOneShot(swordSheatheSound);
        }

        [ObserversRpc(runLocally: true)]
        public async void PlaySwordSwing()
        {
            if (_audioSource == null || swordSwingSound == null) return;
            _lastSwingSoundTime = Time.time;
            
            await System.Threading.Tasks.Task.Delay(100);
            
            if (this != null && _audioSource != null && swordSwingSound != null)
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
