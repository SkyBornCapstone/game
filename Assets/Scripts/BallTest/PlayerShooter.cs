using PurrNet;
using UnityEngine;

namespace BallTest
{
    public class PlayerShooter : NetworkIdentity
    {
        [SerializeField] private Rigidbody projectileRigidbody;
        [SerializeField] private float shootForce = 20;


        private void Update()
        {
            if (!isOwner)
                return;

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                float t = (transform.position.y - ray.origin.y) / ray.direction.y;
                Vector3 hitPoint = ray.origin + ray.direction * t;
                Vector3 dir = (hitPoint - transform.position).normalized;
                Shoot(dir);
            }
        }

        [ServerRpc]
        public void Shoot(Vector3 direction)
        {
            var realDir = new Vector3(direction.x, 0, direction.z).normalized;
            var spawnPosition = transform.position + (realDir * 1.5f);
            var createdObject = Instantiate(projectileRigidbody.gameObject, spawnPosition,
                Quaternion.identity);

            createdObject.TryGetComponent(out Rigidbody rb);
            rb.linearVelocity = realDir * shootForce;
            Debug.Log($"Added Force {realDir * shootForce}. Post-Force Velocity: {rb.linearVelocity}"); //TODO fix
        }
    }
}