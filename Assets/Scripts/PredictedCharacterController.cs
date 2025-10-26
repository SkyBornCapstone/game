using System;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PredictedCharacterController : NetworkBehaviour
{
    private Transform _transform;
    private CharacterController _characterController;
    
    [SerializeField] private float movementSpeed;
    [SerializeField] private float jumpSpeed;
    [SerializeField] private float gravityScale;
    [SerializeField] private float lookSpeed;

    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _lookAction;
    
    private float _horizontalInput;
    private float _verticalInput;
    private bool _isGrounded;
    private bool _jump;
    private float _yawInput;
    
    private Vector3 _velocity;

    public override void OnStartNetwork()
    {
        _transform = transform;

        if (!TryGetComponent(out _characterController)) Debug.LogWarning("CharacterController not found.");

        TimeManager.OnTick += TimeManagerTickEventHandler;
        TimeManager.OnPostTick += TimeManagerPostTickEventHandler;
    }

    public override void OnStopNetwork()
    {
        TimeManager.OnTick -= TimeManagerTickEventHandler;
        TimeManager.OnPostTick -= TimeManagerPostTickEventHandler;
    }

    public override void OnStartClient()
    {
        if (!IsOwner) return;
        
        var playerActionMap = InputSystem.actions.FindActionMap("Player", true);
        
        _moveAction = playerActionMap.FindAction("Move", true);
        _jumpAction = playerActionMap.FindAction("Jump", true);
        _lookAction = playerActionMap.FindAction("Look", true);
    }

    private void Update()
    {
        if (!IsOwner) return;
        
        _horizontalInput = _moveAction.ReadValue<Vector2>().x;
        _verticalInput = _moveAction.ReadValue<Vector2>().y;
        _isGrounded = _characterController.isGrounded;
        _jump = _jumpAction.IsPressed();
        _yawInput = _lookAction.ReadValue<Vector2>().x;
    }
    
    private void TimeManagerPostTickEventHandler()
    {
        if (IsOwner)
        {
            var data = new ReplicationData(_horizontalInput, _verticalInput, _isGrounded, _jump, _yawInput);
            Replicate(data);
        }
        else
        {
            Replicate(default);
        }
    }
    
    private void TimeManagerTickEventHandler()
    {
        CreateReconcile();
    }

    [Replicate]
    private void Replicate(ReplicationData data, ReplicateState state = ReplicateState.Invalid, Channel channel = Channel.Unreliable)
    {
        float tickDelta = (float)TimeManager.TickDelta;
        
        Vector3 desiredVel = (transform.right * data.HorizontalInput + transform.forward * data.VerticalInput) * movementSpeed;
        
        desiredVel = Vector3.ClampMagnitude(desiredVel, movementSpeed);

        _velocity.x = desiredVel.x;
        _velocity.z = desiredVel.z;

        if (data.IsGrounded)
        {
            _velocity.y = 0f;
            
            if (data.Jump)
            {
                _velocity.y = jumpSpeed;
            }
        }
        else
        {
            _velocity.y += Physics.gravity.y * gravityScale * tickDelta;
        }
        
        _characterController.Move(_velocity * tickDelta);
        _transform.Rotate(Vector3.up * (data.YawInput * lookSpeed));
    }

    public override void CreateReconcile()
    {
        transform.GetPositionAndRotation(out Vector3 position, out Quaternion rotation);
        ReconciliationData data = new(position, rotation, _velocity);

        Reconcile(data);
    }

    [Reconcile]
    private void Reconcile(ReconciliationData data, Channel channel = Channel.Unreliable)
    {
        _transform.SetPositionAndRotation(data.Position, data.Rotation);
        _velocity = data.Velocity;
    }

    public struct ReplicationData : IReplicateData
    {
        private uint _tick;

        public readonly float HorizontalInput;
        public readonly float VerticalInput;
        public readonly bool IsGrounded;
        public readonly bool Jump;
        public readonly float YawInput;

        public ReplicationData(float horizontalInput, float verticalInput, bool isGrounded, bool jump, float yawInput): this()
        {
            HorizontalInput = horizontalInput;
            VerticalInput = verticalInput;
            IsGrounded = isGrounded;
            Jump = jump;
            YawInput = yawInput;
        }

        public readonly uint GetTick()
        {
            return _tick;
        }

        public void SetTick(uint value)
        {
            _tick = value;
        }

        public void Dispose()
        {
            // Used internally
        }
    }
    
    public struct ReconciliationData : IReconcileData
    {
        private uint _tick;

        public readonly Vector3 Position;
        public readonly Quaternion Rotation;
        public readonly Vector3 Velocity;

        public ReconciliationData(Vector3 position, Quaternion rotation, Vector3 velocity) : this()
        {
            Position = position;
            Rotation = rotation;
            Velocity = velocity;
        }

        public readonly uint GetTick()
        {
            return _tick;
        }

        public void SetTick(uint value)
        {
            _tick = value;
        }

        public void Dispose()
        {
            // Used internally       
        }
    }
}