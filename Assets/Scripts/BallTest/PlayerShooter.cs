using PurrNet.Prediction;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerShooter : PredictedIdentity<PlayerShooter.Input, PlayerShooter.State>
{
    [SerializeField] private PredictedRigidbody projectileRigidbody;
    [SerializeField] private float shootForce = 20;
    [SerializeField] private float shootTime = 1f;
    [SerializeField] private int maxAmmo = 5;
    [SerializeField] private float reloadTime = 1.5f;

    protected override State GetInitialState()
    {
        var state = new State();

        state.ammo = maxAmmo;
        return state;
    }

    protected override void Simulate(Input input, ref State state, float delta)
    {
        if (state.CanShoot && input.Shoot && state.ammo > 0)
        {
            state.TimeToCanShoot = shootTime;
            state.ammo--;

            if (state.ammo <= 0)
            {
                state.reloadTimer = reloadTime;
            }
            
            var realDir = new Vector3(input.Direction.x, 0, input.Direction.z);
            var spawnPosition = transform.position + realDir;
            var createdObject = predictionManager.hierarchy.Create(projectileRigidbody.gameObject, spawnPosition, Quaternion.identity);
            if (!createdObject.HasValue)
                return;

            createdObject.Value.TryGetComponent(predictionManager, out PredictedRigidbody rb);
            rb.AddForce(realDir * shootForce, ForceMode.Impulse);
        }
        
        if (!state.CanShoot)
            state.TimeToCanShoot -= delta;

        if (state.reloadTimer > 0)
        {
            state.reloadTimer -= delta;
            if (state.reloadTimer <= 0)
                state.ammo = maxAmmo;
        }
    }

    protected override void UpdateInput(ref Input input)
    {
        input.Shoot |= UnityEngine.Input.GetKeyDown(KeyCode.Mouse0);
    }

    protected override void GetFinalInput(ref Input input)
    {
        Ray ray = Camera.main.ScreenPointToRay(UnityEngine.Input.mousePosition);
        float t = (transform.position.y - ray.origin.y) / ray.direction.y;
        Vector3 hitPoint = ray.origin + ray.direction * t;
        input.Direction = hitPoint - transform.position;
    }

    protected override void SanitizeInput(ref Input input)
    {
        input.Direction.Normalize();
    }

    public struct State : IPredictedData<State>
    {
        public float TimeToCanShoot;
        public bool CanShoot => TimeToCanShoot <= 0;
        public float reloadTimer;
        public int ammo;
        
        public void Dispose() { }
    }

    public struct Input : IPredictedData
    {
        public bool Shoot;
        public Vector3 Direction;
        
        public void Dispose() { }
    }
}