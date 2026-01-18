using PurrNet;
using UnityEngine;

namespace BallTest
{
    public class Projectile : NetworkIdentity
    {
        [SerializeField] private Rigidbody rb;
        [SerializeField] private int damage = 20;
        [SerializeField] private float lifetime = 5;

        private float ttl;

        private int _frames = 0;

        private void Awake()
        {
            ttl = lifetime;
        }

        private void FixedUpdate()
        {
            _frames++;
            if (_frames <= 5)
            {
                Debug.Log(
                    $"[Frame {_frames}] Velocity: {rb.linearVelocity} (Mag: {rb.linearVelocity.magnitude}) | Pos: {transform.position}");
                //TODO fix
            }

            if (!isServer) return;

            ttl -= Time.fixedDeltaTime;

            if (ttl <= 0)
            {
                Destroy(gameObject);
            }
        }

        private void OnCollisionEnter(Collision other)
        {
            if (!isServer) return;

            if (!other.transform.TryGetComponent(out PlayerHealth playerHealth))
                return;

            playerHealth.TakeDamage(damage);
        }
    }
}