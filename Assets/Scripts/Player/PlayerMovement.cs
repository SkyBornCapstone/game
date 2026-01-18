using PurrNet;
using UnityEngine;

namespace Player
{
    public class PlayerMovement : NetworkIdentity
    {
        [Header("Movement")] [SerializeField] private float moveSpeed = 5;
        [SerializeField] private float sprintSpeed = 8;
        [SerializeField] private float acceleration = 20;
        [SerializeField] private float planarDamping = 10f;
        [SerializeField] private float jumpForce = 7f;
        [SerializeField] private float groundCheckRadius = 0.2f;
        [SerializeField] private LayerMask groundLayer;

        [Header("References")] [SerializeField]
        private Rigidbody rb;

        [SerializeField] private NetworkAnimator animator;

        private static readonly int VelocityXHash = Animator.StringToHash("Velocity X");
        private static readonly int VelocityZHash = Animator.StringToHash("Velocity Z");
        private static readonly int JumpHash = Animator.StringToHash("Jump");
        private static readonly int IsGroundedHash = Animator.StringToHash("Is Grounded");

        [Header("Ship Interaction Variables")] public bool isUsingShip;
        private Transform shipAnchor;
        [Header("Cannon Variables")] public bool isUsingCannon;
        private Transform cannonSeat;

        private Vector3 velocity;
        private float verticalRotation = 0f;

        private bool _jump;

        protected override void OnSpawned()
        {
            enabled = isOwner;
        }

        private void Update()
        {
            bool isGrounded = IsGrounded();

            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                animator.SetBool(JumpHash, true);
            }
            else
            {
                animator.SetBool(JumpHash, false);
            }

            Vector3 localVelocity = transform.InverseTransformDirection(rb.linearVelocity);

            animator.SetFloat(VelocityXHash, localVelocity.x);
            animator.SetFloat(VelocityZHash, localVelocity.z);
            animator.SetBool(IsGroundedHash, isGrounded);
        }

        private void FixedUpdate()
        {
            Vector2 moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

            float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : moveSpeed;

            Vector3 targetVel =
                (transform.forward * moveInput.y + transform.right * moveInput.x) *
                currentSpeed;
            rb.AddForce(targetVel * acceleration);

            var horizontal = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            rb.AddForce(-horizontal * planarDamping);
            if (horizontal.magnitude > currentSpeed)
                rb.linearVelocity = new Vector3(targetVel.x, rb.linearVelocity.y, targetVel.z);
        }

        private static readonly Collider[] _groundCheckColliders = new Collider[16];

        private bool IsGrounded()
        {
            var hit = Physics.OverlapSphereNonAlloc(transform.position, groundCheckRadius, _groundCheckColliders,
                groundLayer);
            return hit > 0;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, groundCheckRadius);
        }
    }
}