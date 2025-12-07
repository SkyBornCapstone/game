using System;
using PurrNet.Prediction;
using UnityEngine;
using balltest;
public class Projectile : PredictedIdentity<Projectile.State>
{
    [SerializeField] private PredictedRigidbody predictedRigidbody;
    [SerializeField] private int damage = 20;
    [SerializeField] private float lifetime = 5;

    private void OnEnable()
    {
        predictedRigidbody.onCollisionEnter += OnHit;
    }

    private void OnDisable()
    {
        predictedRigidbody.onCollisionEnter -= OnHit;
    }

    protected override State GetInitialState()
    {
        var state = new State();
        state.TimeToLive = lifetime;
        return state;
    }

    private void OnHit(GameObject other, PhysicsCollision physicsEvent)
    {
        if (!other.TryGetComponent(out PlayerHealth playerHealth))
            return;
        
        playerHealth.TakeDamage(damage);
        predictionManager.hierarchy.Delete(gameObject);
    }

    protected override void Simulate(ref State state, float delta)
    {
        if (state.TimeToLive > 0)
        {
            state.TimeToLive -= delta;
        }
        else
        {
            predictionManager.hierarchy.Delete(gameObject);
        }
    }

    public struct State : IPredictedData<State>
    {
        public float TimeToLive;
        
        public void Dispose() { }
    }
}
