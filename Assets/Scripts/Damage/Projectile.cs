using System;
using UnityEngine;
using PurrNet.Prediction;
using player;

namespace Damage
{
    public class Projectile : PredictedIdentity<Projectile.State>
    {
        [SerializeField] private int damage = 20;
        [SerializeField] private bool destroyOnHit = true;
        [SerializeField] private PredictedRigidbody predictedRigidbody;

        private void OnEnable()
        {
            predictedRigidbody.onCollisionEnter += OnHit;
            
            // predictedRigidbody.onTriggerEnter += OnHit;
        }

        private void OnDisable()
        {
            predictedRigidbody.onCollisionEnter -= OnHit;
            // predictedRigidbody.onTriggerEnter -= OnHit;
        }


        private void OnHit(GameObject other, PhysicsCollision collision)
        {
            // Debug.Log($"TryGetComponent: {other.TryGetComponent(out PlayerHealth playerHalth)} on object: {other.name}");
            Debug.Log("Projectile collided with: " + other.name);
            if (other.TryGetComponent(out PlayerHealth playerHealth))
            {
                playerHealth.TakeDamage(damage);
            }

            if (destroyOnHit)
            {
                predictionManager.hierarchy.Delete(gameObject);
            }
        }

        protected override State GetInitialState()
        {
            return new State();
        }
        
        

        public struct State : IPredictedData<State>
        {
            public void Dispose() { }
        }
    }
}