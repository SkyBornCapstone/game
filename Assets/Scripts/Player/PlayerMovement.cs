using PurrNet.Prediction;
using UnityEngine;

namespace Player
{
    public class PlayerMovement : PredictedIdentity<PlayerMovement.MoveInput, PlayerMovement.MoveState>
    {
        [SerializeField] private float moveSpeed = 7;
        [SerializeField] private float acceleration = 20;
        [SerializeField] private float planarDamping = 10f;
        [SerializeField] private float jumpForce = 7f;
        [SerializeField] private float groundCheckRadius = 0.2f;
        [SerializeField] private LayerMask groundLayer;

        [SerializeField] private new FirstPersonCamera camera;
        [SerializeField] private PredictedRigidbody predictedRigidbody;
        [SerializeField] private Animator animator;

        private static readonly int VelocityXHash = Animator.StringToHash("Velocity X");
        private static readonly int VelocityZHash = Animator.StringToHash("Velocity Z");
        private static readonly int JumpHash = Animator.StringToHash("Jump");
        private static readonly int IsGroundedHash = Animator.StringToHash("Is Grounded");

        [Header("Ship Interaction Variables")]
        public bool isUsingShip;
        private Transform shipAnchor;
        [Header("Cannon Variables")] public bool isUsingCannon;
        private Transform cannonSeat;
        

        protected override void LateAwake()
        {
            if (isOwner)
                camera.Init();
        }

        protected override void Simulate(MoveInput moveInput, ref MoveState moveState, float delta)
        {
            if (isUsingShip || isUsingCannon)
            {
                // Lock position when using ship
                if (isUsingShip && shipAnchor != null)
                {
                    predictedRigidbody.position = shipAnchor.position;
                }
                // Lock position when using cannon
                else if (isUsingCannon && cannonSeat != null)
                predictedRigidbody.velocity = Vector3.zero;
                predictedRigidbody.angularVelocity = Vector3.zero;
                moveState.velocity = Vector3.zero;
                moveState.isGrounded = true;
                moveState.jump = false;
        
                // Still allow rotation while interacting
                if (moveInput.cameraForward.HasValue)
                {
                    var camForward = moveInput.cameraForward.Value;
                    camForward.y = 0;
                    if (camForward.sqrMagnitude > 0.0001f)
                        predictedRigidbody.MoveRotation(Quaternion.LookRotation(camForward.normalized));
                }
                return;
            }
            Vector3 targetVel =
                (transform.forward * moveInput.moveDirection.y + transform.right * moveInput.moveDirection.x) *
                moveSpeed;
            predictedRigidbody.AddForce(targetVel * acceleration);

            var horizontal = new Vector3(predictedRigidbody.linearVelocity.x, 0, predictedRigidbody.linearVelocity.z);
            predictedRigidbody.AddForce(-horizontal * planarDamping);
            if (horizontal.magnitude > moveSpeed)
                predictedRigidbody.velocity = new Vector3(targetVel.x, predictedRigidbody.velocity.y, targetVel.z);

            // moveState.isWalking = horizontal.sqrMagnitude > 0.0001f;
            moveState.velocity = horizontal;
            var isGrounded = IsGrounded();
            moveState.isGrounded = isGrounded;

            if (moveInput.jump && isGrounded)
            {
                predictedRigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                moveState.jump = true;
            }
            else
            {
                moveState.jump = false;
            }

            if (moveInput.cameraForward.HasValue)
            {
                var camForward = moveInput.cameraForward.Value;
                camForward.y = 0;
                if (camForward.sqrMagnitude > 0.0001f)
                    predictedRigidbody.MoveRotation(Quaternion.LookRotation(camForward.normalized));
            }
        }

        protected override void UpdateView(MoveState viewState, MoveState? verified)
        {
            if (!animator)
                return;

            if (isOwner || !verified.HasValue)
            {
                UpdateAnimator(viewState);
            }
            else
            {
                UpdateAnimator(verified.Value);
            }
        }

        private void UpdateAnimator(MoveState state)
        {
            Vector3 worldVelocity = state.velocity;

            Vector3 localVelocity = transform.InverseTransformDirection(worldVelocity);

            float deltaTime = Time.deltaTime;
            animator.SetFloat(VelocityXHash, localVelocity.x, .1f, deltaTime);
            animator.SetFloat(VelocityZHash, localVelocity.z, .1f, deltaTime);
            animator.SetBool(IsGroundedHash, state.isGrounded);

            if (state.jump)
            {
                animator.SetBool(JumpHash, true);
            }
            else
            {
                animator.SetBool(JumpHash, false);
            }
        }

        protected override void UpdateInput(ref MoveInput input)
        {
            if (isUsingShip || isUsingCannon)
            {
                input.moveDirection = Vector2.zero;
                input.jump = false;
                return;
            }
            
            input.jump |= Input.GetKeyDown(KeyCode.Space);
        }

        private static readonly Collider[] _groundCheckColliders = new Collider[16];

        private bool IsGrounded()
        {
            var hit = Physics.OverlapSphereNonAlloc(transform.position, groundCheckRadius, _groundCheckColliders,
                groundLayer);
            return hit > 0;
        }

        protected override void GetFinalInput(ref MoveInput moveInput)
        {
            if (isUsingShip || isUsingCannon)
            {
                moveInput.moveDirection = Vector2.zero;
                moveInput.cameraForward = camera.Forward;
                return;
            }
            moveInput.moveDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            moveInput.cameraForward = camera.Forward;
        }

        protected override void SanitizeInput(ref MoveInput input)
        {
            if (input.moveDirection.sqrMagnitude > 1)
                input.moveDirection.Normalize();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, groundCheckRadius);
        }

        public struct MoveState : IPredictedData<MoveState>
        {
            public Vector3 velocity;
            public bool jump;
            public bool isGrounded;

            public void Dispose()
            {
            }
        }

        public struct MoveInput : IPredictedData
        {
            public Vector2 moveDirection;
            public Vector3? cameraForward;
            public bool jump;

            public void Dispose()
            {
            }
        }

        public void EnterShip(Transform anchor)
        {
            isUsingShip = true;
            shipAnchor = anchor;
            if (!predictedRigidbody.isKinematic)
            {
                predictedRigidbody.velocity = Vector3.zero;
                predictedRigidbody.angularVelocity = Vector3.zero;
            }
        }

        public void ExitShip()
        {
            isUsingShip = false;
            shipAnchor = null;
        }
        public void EnterCannon(Transform seat)
        {
            isUsingCannon = true;
            cannonSeat = seat;
            transform.position = cannonSeat.position;
            predictedRigidbody.angularVelocity = Vector3.zero;
            transform.rotation = cannonSeat.rotation;
            
            predictedRigidbody.velocity = Vector3.zero;
            
        }

        public void ExitCannon()
        {
            
            isUsingCannon = false;
            cannonSeat = null;
        }
    }
}