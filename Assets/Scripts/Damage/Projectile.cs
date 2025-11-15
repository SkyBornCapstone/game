using System;
using UnityEngine;

namespace Damage
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private int damage = 10;
        [SerializeField] private float speed = 20f;
        [SerializeField] private float lifetime = 5f;
        [SerializeField] private LayerMask hitLayers;
        [SerializeField] private bool destroyOnHit = true;
        
        private Rigidbody rb;
        private float spawnTime;
        
        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            spawnTime = Time.time;
        }
        
        private void Start()
        {
            if (rb != null)
            {
                rb.linearVelocity = transform.forward * speed;
            }
        }
        
        private void Update()
        {
            if (Time.time - spawnTime >= lifetime)
            {
                Destroy(gameObject);
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {

            if (((1 << other.gameObject.layer) & hitLayers) == 0)
                return;
            
            PlayerHealth health = other.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }
            if (destroyOnHit)
            {
                Destroy(gameObject);
            }
        }
        public void Initialize(int projectileDamage, float projectileSpeed, float projectileLifetime)
        {
            damage = projectileDamage;
            speed = projectileSpeed;
            lifetime = projectileLifetime;
            
            spawnTime = Time.time;
            
            if (rb != null)
            {
                rb.linearVelocity = transform.forward * speed;
            }
        }
    }
}