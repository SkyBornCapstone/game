using System;
using UnityEngine;

public class ShipPhysics : MonoBehaviour
{
    // Physics Properties
    public float baseMass = 1000f;     // kg
    public float baseThrust = 0f;
    public float gravity = 9.81f;      // m/s²

    // Dynamic Variables
    public float currentMass;
    public float currentThrust;
    public float verticalVelocity;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentMass = baseMass;
        currentThrust = baseThrust;
    }

    // Update is called once per frame
    void Update()
    {
        ApplyCustomPhysics(Time.deltaTime);
    }


    void ApplyCustomPhysics(float deltaTime)
    {
        float weightForce = currentMass * gravity;
        float netForce = currentThrust - weightForce;
        float liftRatio = netForce / weightForce;
        float accelerationFactor = liftRatio * liftRatio * liftRatio;
        float acceleration = currentMass * (float)Math.Log(1 + accelerationFactor);

        verticalVelocity += acceleration * deltaTime;

        transform.position += Vector3.up * verticalVelocity * deltaTime;
    }



    // Modifiers
    public void AddThrust(float extraThrust)
    {
        currentThrust += extraThrust;
    }
    
    public void RemoveThrust(float thrust)
    {
        currentThrust = Mathf.Max(0, currentThrust - thrust);
    }

    public void AddMass(float extraMass)
    {
        currentMass += extraMass;
    }

    public void RemoveMass(float mass)
    {
        currentMass = Mathf.Max(baseMass, currentMass - mass);
    }
}
