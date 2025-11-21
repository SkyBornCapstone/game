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
        // X axis limit
        public float maxPitchAngle = 10f; 
        // Z axis limit
        public float maxRollAngle = 20f; 
        // Strength for roll auto-leveling (Z axis)
        public float rollLevelStrength = 1f;

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
            // Include inactive engines in case they're disabled in the hierarchy
            engines = GetComponentsInChildren<ShipEngine>(true);
            if (engines == null || engines.Length == 0)
            {
                Debug.LogWarning("[ShipPhysics] No ShipEngine components found as children (includeInactive=true). Physics will behave as if no thrust is available.");
            }
        }

        void FixedUpdate()
        {
            ApplyCustomPhysics(Time.fixedDeltaTime);
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
                // normalize pitch to -180..180 for meaningful signed torque
                float signedPitch = currentPitch > 180f ? currentPitch - 360f : currentPitch;
                float levelTorque = -signedPitch * levelStrength;
                angularVelocity.x += levelTorque * deltaTime;
                // Roll auto-leveling: apply proportional torque to reduce roll towards zero
                float currentRoll = transform.eulerAngles.z;
                float signedRoll = currentRoll > 180f ? currentRoll - 360f : currentRoll;
                float rollTorque = -signedRoll * rollLevelStrength;
                angularVelocity.z += rollTorque * deltaTime;
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

            // Enforce rotation limits and prevent deathball spinning
            float pitch = transform.eulerAngles.x;
            float signedPitchFinal = pitch > 180f ? pitch - 360f : pitch;
            float clampedPitch = Mathf.Clamp(signedPitchFinal, -maxPitchAngle, maxPitchAngle);

            float roll = transform.eulerAngles.z;
            float signedRollFinal = roll > 180f ? roll - 360f : roll;
            float clampedRoll = Mathf.Clamp(signedRollFinal, -maxRollAngle, maxRollAngle);

            if (!Mathf.Approximately(clampedPitch, signedPitchFinal) || !Mathf.Approximately(clampedRoll, signedRollFinal))
            {
                // Convert clamped signed angles back to 0..360 representation for Euler set
                Vector3 e = transform.eulerAngles;
                e.x = clampedPitch < 0f ? 360f + clampedPitch : clampedPitch;
                e.z = clampedRoll  < 0f ? 360f + clampedRoll  : clampedRoll;
                transform.eulerAngles = e;

                // If we hit limits, zero any angular velocity pushing further out of bounds
                if (signedPitchFinal > clampedPitch && angularVelocity.x > 0f) angularVelocity.x = 0f;
                if (signedPitchFinal < clampedPitch && angularVelocity.x < 0f) angularVelocity.x = 0f;
                if (signedRollFinal  > clampedRoll  && angularVelocity.z > 0f) angularVelocity.z = 0f;
                if (signedRollFinal  < clampedRoll  && angularVelocity.z < 0f) angularVelocity.z = 0f;
            }

            // Update position
            transform.position += velocity * deltaTime;
        }

        //Modifiers
        public void AddMass(float extraMass) => currentMass += extraMass;
        public void RemoveMass(float extraMass) => currentMass = Mathf.Max(baseMass, currentMass - extraMass);
    }
}