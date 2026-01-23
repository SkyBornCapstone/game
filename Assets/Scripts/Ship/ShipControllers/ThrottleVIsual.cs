using UnityEngine;

public class ThrottleVIsual : MonoBehaviour
{
    public ShipControllerV2 ship;
    public Transform throttleVisual;
    
    public float maxRotation = 270f;
    public float throttleTurnRate = 90f;
    private Quaternion initialRotation;
    public float CurrentAngle => currentAngle;
    private float currentAngle;
    private Quaternion currentRotation;
    private float minRotation = 0f;
    void Awake()
    {
        initialRotation = throttleVisual.localRotation;
        print("HERE");
        print(initialRotation);
    }

    public void IncreaseThrottle()
    {
        currentAngle += throttleTurnRate * Time.deltaTime;
        currentAngle = Mathf.Clamp(currentAngle, minRotation, maxRotation);
    }

    public void DecreaseThrottle()
    {
        currentAngle -= throttleTurnRate * Time.deltaTime;
        currentAngle = Mathf.Clamp(currentAngle, minRotation, maxRotation);
    }

    void Update()
    {
        if (!throttleVisual)
            return;
        throttleVisual.localRotation = initialRotation * Quaternion.Euler(0f, currentAngle, 0f); 
    }
}
