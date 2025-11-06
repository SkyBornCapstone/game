using System;
using PurrNet.Prediction;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace BallTest
{
    public class PlayerMovement : PredictedIdentity<PlayerMovement.Input, PlayerMovement.State>
    {
        [SerializeField] protected PredictedTransform predictedTransform;
        [SerializeField] private PredictedRigidbody predictedRigidbody;
        [SerializeField] private float moveForce = 5; 
        [SerializeField] private float jumpForce = 10;
        [SerializeField] private float knockbackForce = 4;
        [SerializeField] private float groundCheckDistance = 0.51f;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private PlayerHealth playerHealth;

        protected override void LateAwake()
        {
            base.LateAwake();

            if (isOwner)
            {
                PlayerCamera.Instance.SetTarget(predictedTransform.graphics);
            }
        }

        private void OnEnable()
        {
            predictedRigidbody.onCollisionEnter += OnCollisionStart;
        }

        private void OnDisable()
        {
            predictedRigidbody.onCollisionEnter -= OnCollisionStart;
        }

        private void OnCollisionStart(GameObject other, PhysicsCollision physicsEvent)
        {
            if (!other.TryGetComponent(out PlayerMovement otherPlayer))
                return;
        
            var dir = (transform.position - otherPlayer.transform.position).normalized;
            predictedRigidbody.AddForce(dir * knockbackForce, ForceMode.Impulse);
            playerHealth.HitOtherPlayer();
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
}