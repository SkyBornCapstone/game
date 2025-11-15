using System;
using UnityEngine;

namespace Ship
{
    [DisallowMultipleComponent]
    public class ShipPhysics : MonoBehaviour
    {
        [Header("Ship Properties")]
        public float baseMass = 1000f;
        public float gravity = 9.81f;
        public float rotationalDamping = 0.98f;
        public float linearDamping = 0.995f;
        public float levelStrength = 10f;
        public bool autoLevel = true;

        [Header("Runtime State")]
        public float currentMass;
        public float verticalVelocity;
        public Vector3 velocity;
        public Vector3 angularVelocity;
        public float turnSensitivity = 0.05f;

        private ShipEngine[] engines;

        void Start()
        {
            currentMass = baseMass;
            engines = GetComponentsInChildren<ShipEngine>();
        }

        void Update()
        {
            ApplyCustomPhysics(Time.deltaTime);
        }

        void ApplyCustomPhysics(float deltaTime)
        {
            Vector3 totalForce = Vector3.zero;

            float leftForce = 0f;
            float rightForce = 0f;

            foreach (var engine in engines)
            {
                Vector3 thrust = engine.GetWorldThrustForce();
                if (engine.engineID == "L")
                    leftForce += thrust.magnitude;
                else if (engine.engineID == "R")
                    rightForce += thrust.magnitude;
                else
                    totalForce += thrust;
            }
            // Transform left and right forces into forward thrust
            Vector3 forwardThrust = transform.forward * (leftForce + rightForce);
            totalForce += forwardThrust;

            // Turning based on left/right force difference
            float turnForce = (leftForce - rightForce) * turnSensitivity;
            angularVelocity.y += turnForce * deltaTime;

            // Roll to the side a bit when turning
            angularVelocity.z += turnForce * -0.2f * deltaTime;

            // Auto-leveling to keep ship level (front-to-back)
            if (autoLevel)
            {
                float currentPitch = transform.eulerAngles.x;
                float levelTorque = -currentPitch * levelStrength;
                angularVelocity.x += levelTorque * deltaTime;
            }

            // Custom lift calculation
            float totalThrustY = Vector3.Dot(totalForce, Vector3.up);
            float weightForce = currentMass * gravity;
            float netForce = totalThrustY - weightForce;

            float liftRatio = netForce / weightForce;
            float accelFactor = liftRatio * liftRatio * liftRatio;
            float liftAcceleration = (float)Math.Log(1 + Math.Abs(accelFactor)) * Mathf.Sign(liftRatio);

            verticalVelocity += liftAcceleration * deltaTime;

            Vector3 horizontalForce = totalForce - Vector3.up * totalThrustY;
            Vector3 horizontalAcceleration = horizontalForce / currentMass;

            // Combine vertical + horizontal velocity
            velocity += (horizontalAcceleration + Vector3.up * verticalVelocity) * deltaTime;

            // Apply damping
            velocity *= linearDamping;
            angularVelocity *= rotationalDamping;

            // Update rotation
            transform.Rotate(Vector3.right, angularVelocity.x * deltaTime);
            transform.Rotate(Vector3.up, angularVelocity.y * deltaTime);
            transform.Rotate(Vector3.forward, angularVelocity.z * deltaTime);

            // Update position
            transform.position += velocity * deltaTime;
        }

        //Modifiers
        public void AddMass(float extraMass) => currentMass += extraMass;
        public void RemoveMass(float extraMass) => currentMass = Mathf.Max(baseMass, currentMass - extraMass);
    }
}