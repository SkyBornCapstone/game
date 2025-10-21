using UnityEngine;
using System;

[DisallowMultipleComponent]
public class ShipController : MonoBehaviour
{
    [Header("Ship Properties")]
    public float baseMass = 1000f;
    public float gravity = 9.81f;
    public float rotationalDamping = 0.98f;
    public float linearDamping = 0.995f;

    [Header("Runtime State")]
    public float currentMass;
    public float verticalVelocity;
    public Vector3 velocity;
    public Vector3 angularVelocity; // pitch/yaw/roll velocity

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
        // Collect forces & torques
        Vector3 totalForce = Vector3.zero;
        Vector3 totalTorque = Vector3.zero;

        foreach (var engine in engines)
        {
            Vector3 thrust = engine.GetWorldThrustForce();
            totalForce += thrust;

            // Torque = r × F
            Vector3 leverArm = engine.GetRelativePosition(transform);
            Vector3 torque = Vector3.Cross(leverArm, thrust);
            totalTorque += torque;
        }

        // Gravity
        Vector3 gravityForce = Vector3.down * currentMass * gravity;
        totalForce += gravityForce;

        // Split lift component
        float totalThrustY = Vector3.Dot(totalForce, Vector3.up);
        float weightForce = currentMass * gravity;
        float netForce = totalThrustY - weightForce;

        float liftRatio = netForce / weightForce;
        float accelFactor = liftRatio * liftRatio * liftRatio;
        float liftAcceleration = (float)Math.Log(1 + Math.Abs(accelFactor)) * Mathf.Sign(liftRatio);

        // Apply lift only to vertical velocity
        verticalVelocity += liftAcceleration * deltaTime;

        // Compute linear & rotational motion
        // Horizontal acceleration = remaining horizontal force / mass
        Vector3 horizontalForce = totalForce - Vector3.up * totalThrustY;
        Vector3 horizontalAcceleration = horizontalForce / currentMass;

        // Combine vertical + horizontal velocity
        velocity += (horizontalAcceleration + Vector3.up * liftAcceleration) * deltaTime;

        // Apply damping
        velocity *= linearDamping;
        verticalVelocity *= linearDamping;

        // Update position 
        transform.position += velocity * deltaTime;

        // Simplified rotational inertia approximation:
        Vector3 angularAcceleration = totalTorque / (currentMass * 0.1f);
        angularVelocity += angularAcceleration * deltaTime;
        angularVelocity *= rotationalDamping;

        // Apply rotation
        transform.Rotate(angularVelocity * Mathf.Rad2Deg * deltaTime, Space.Self);
    }

    //Modifiers
    public void AddMass(float extraMass) => currentMass += extraMass;
    public void RemoveMass(float extraMass) => currentMass = Mathf.Max(baseMass, currentMass - extraMass);
}