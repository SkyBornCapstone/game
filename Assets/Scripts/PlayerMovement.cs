using PurrNet.Prediction;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerMovement : PredictedIdentity<PlayerMovement.Input, PlayerMovement.State>
{
    [SerializeField] protected PredictedTransform predictedTransform;
    [SerializeField] private PredictedRigidbody predictedRigidbody;
    [SerializeField] private float moveForce = 5; 
    [SerializeField] private float jumpForce = 10;
    [SerializeField] private float groundCheckDistance = 0.51f;
    [SerializeField] private LayerMask groundLayer;

    protected override void LateAwake()
    {
        base.LateAwake();

        if (isOwner)
        {
            PlayerCamera.Instance.SetTarget(predictedTransform.graphics);
        }
    }

    protected override void Simulate(Input input, ref State state, float delta)
    {
        Vector3 moveDir = new Vector3(input.Direction.x, 0, input.Direction.y).normalized * moveForce;
        predictedRigidbody.AddForce(moveDir);
        
        if (input.Jump && IsGrounded())
        {
            predictedRigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    protected override void GetFinalInput(ref Input input)
    {
        input.Direction = new Vector2(UnityEngine.Input.GetAxisRaw("Horizontal"), UnityEngine.Input.GetAxisRaw("Vertical"));
        
    }

    protected override void UpdateInput(ref Input input)
    {
        input.Jump |= UnityEngine.Input.GetKeyDown(KeyCode.Space);
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, out var hit, groundCheckDistance, groundLayer);
    }

    public struct State : IPredictedData<State>
    {
        public void Dispose() { }
    }

    public struct Input : IPredictedData
    {
        public Vector2 Direction;
        public bool Jump;
        
        public void Dispose() { }
    }
}
