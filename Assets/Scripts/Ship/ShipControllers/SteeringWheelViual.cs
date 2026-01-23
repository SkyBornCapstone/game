using Unity.VisualScripting;
using UnityEngine;

public class SteeringWheelViual : MonoBehaviour
{
    public ShipControllerV2 ship;
    public Transform wheelVisual;
    
    public float maxRotation = 360f;
    public float wheelTurnRate = 90f; // Degrees per second
    [Range(0f, 1f)] public float deadZone = 0.15f;
    
    public float CurrentAngle => currentAngle;
    private float currentAngle; // This is now independent
    private Quaternion initialRotation;

    void Awake()
    {
        if(wheelVisual)
            initialRotation = wheelVisual.localRotation;
    }
    
    public void TurnWheelRight()
    {
        currentAngle -= wheelTurnRate * Time.deltaTime;
        currentAngle = Mathf.Clamp(currentAngle, -maxRotation, maxRotation);
    }
    
    public void TurnWheelLeft()
    {
        currentAngle += wheelTurnRate * Time.deltaTime;
        currentAngle = Mathf.Clamp(currentAngle, -maxRotation, maxRotation);
    }
    
    void Update()
    {
        if (!wheelVisual)
            return;
     
        wheelVisual.localRotation = initialRotation * Quaternion.Euler(currentAngle, 0f, 0f); 
    }
}
