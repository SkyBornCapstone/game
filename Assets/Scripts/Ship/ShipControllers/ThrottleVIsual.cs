using UnityEngine;

public class ThrottleVIsual : MonoBehaviour
{
    public ShipControllerV2 ship;
    public Transform throttleVisual;

    public float maxRotation;

    private Quaternion initialRotation;

    void Awake()
    {
        initialRotation = throttleVisual.localRotation;
    }
    void Update()
    {
        float throttle = ship.ForwardThrottle;

        float angle = throttle * maxRotation;
        
        throttleVisual.localRotation =  initialRotation * Quaternion.Euler(angle, 0f, 0f);
    }
}
