using UnityEngine;

public class ShipControllerV2 : MonoBehaviour
{
    [Header("Main Ship Variables")] public Transform mainShip;
    public Rigidbody mainShipRB;
   
    [Header("Movement Forces")]
    public float maxLiftForce = 1f;
    public float maxForwardForce = 1f;

    [Header("Throttle Rates")]
    public float liftChangeRate = 0.1f;
    public float forwardChargeRate = 0.1f;

    [Header("Rotation")]
    public float maxYawTorque = 1f;
    public float yawChangeRate = 0.1f;

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
        float yawTorque = yawThrottle * maxYawTorque;
        mainShipRB.AddTorque(
            Vector3.up * yawTorque,
            ForceMode.Force
        );
    }

    private void ApplyForward()
    {
        float forwardForce = forwardThrottle * maxForwardForce;
        mainShipRB.AddForce(
            transform.right * forwardForce,
            ForceMode.Force);
    }

    private void ApplyLift()
    {
        float liftForce = liftThrottle * maxLiftForce;
        mainShipRB.AddForce(Vector3.up * liftForce, ForceMode.Force);
    }

    public void IncreaseForward()
    {
        forwardThrottle += forwardChargeRate * Time.deltaTime;
        forwardThrottle = Mathf.Clamp(forwardThrottle, -1f, 1f);
    }
    
    public void DecreaseForward()
    {
        forwardThrottle -= forwardChargeRate * Time.deltaTime;
        forwardThrottle = Mathf.Clamp(forwardThrottle, -1f, 1f);
    }
    
    public void IncreaseLift()
    {
        liftThrottle += liftChangeRate * Time.deltaTime;
        liftThrottle = Mathf.Clamp(liftThrottle, -1f, 1f);
       
    }

    public void DecreaseLift()
    {
        liftThrottle -= liftChangeRate * Time.deltaTime;
        liftThrottle = Mathf.Clamp(liftThrottle, -1f, 1f);
        print(liftThrottle);
    }
    public void TurnRight()
    {
        yawThrottle += yawChangeRate * Time.deltaTime;
        yawThrottle = Mathf.Clamp(yawThrottle, -1f, 1f);
    }

    public void TurnLeft()
    {
        yawThrottle -= yawChangeRate * Time.deltaTime;
        yawThrottle = Mathf.Clamp(yawThrottle, -1f, 1f);
    }
    
    public void CenterYaw()
    {
        yawThrottle = Mathf.MoveTowards(yawThrottle, 0f, yawChangeRate * Time.deltaTime);
    }


}
