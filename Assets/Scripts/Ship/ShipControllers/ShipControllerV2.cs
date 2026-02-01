using PurrNet;
using UnityEngine;

namespace Ship.ShipControllers
{
    public class ShipControllerV2 : NetworkBehaviour
    {
        [Header("Main Ship Variables")] public Transform mainShip;
        public Rigidbody mainShipRB;

        [Header("Movement Velocities")] public float maxLiftForce = 1f;
        public float maxForwardForce = 1f;

        [Header("Throttle Rates")] public float liftChangeRate = 0.1f;
        public float forwardChargeRate = 0.1f;

        [Header("Rotation")] public float maxYawTorque = 10f;
        public float yawChangeRate = 0.1f;

        [Header("Acceleration")] public float forwardAcceleration = 2f;

        private float liftThrottle;
        private float forwardThrottle;
        private float yawThrottle;

        private void FixedUpdate()
        {
            ApplyLift();
            ApplyForward();
            ApplyYaw();
        }

        private void ApplyYaw()
        {
            mainShipRB.angularVelocity =
                Vector3.up * (yawThrottle * maxYawTorque);
        }

        private void ApplyForward()
        {
            Vector3 currentVelocity = mainShipRB.linearVelocity;

            // Current speed along the ship's forward axis
            float currentForwardSpeed =
                Vector3.Dot(currentVelocity, transform.right);

            // Desired speed from throttle
            float targetForwardSpeed =
                forwardThrottle * maxForwardForce;

            // Smooth acceleration toward target speed
            float newForwardSpeed = Mathf.MoveTowards(
                currentForwardSpeed,
                targetForwardSpeed,
                forwardAcceleration * Time.fixedDeltaTime
            );

            // Rebuild velocity
            Vector3 forwardVelocity = transform.right * newForwardSpeed;

            mainShipRB.linearVelocity = new Vector3(
                forwardVelocity.x,
                currentVelocity.y,
                forwardVelocity.z
            );
        }


        private void ApplyLift()
        {
            Vector3 currentVelocity = mainShipRB.linearVelocity;

            mainShipRB.linearVelocity = new Vector3(
                currentVelocity.x,
                liftThrottle * maxLiftForce,
                currentVelocity.z
            );
        }

        [ServerRpc]
        public void SetYawThrottle(float value)
        {
            yawThrottle = Mathf.Clamp(value, -1f, 1f);
        }

        [ServerRpc]
        public void SetForwardThrottle(float value)
        {
            forwardThrottle = Mathf.Clamp(value, -1f, 1f);
        }

        [ServerRpc]
        public void SetLiftThrottle(float value)
        {
            liftThrottle = Mathf.Clamp(value, -1f, 1f);
        }
    }
}