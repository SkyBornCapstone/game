using Ship;
using JetBrains.Annotations;
using PurrNet;
using UnityEngine;

namespace Player
{
    public class PlayerMovement : NetworkBehaviour
    {
        [SerializeField] private Transform physicsRoot;
        [SerializeField] private Transform visualRoot;
        
        public Transform PhysicsRoot => physicsRoot;
        public Transform VisualRoot => visualRoot;
    
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5;
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

        [Header("Ship Interaction Variables")]
        public bool isOnProxyShip;
        public bool isOnShipDeck;

        private Transform _lockedPosition;

        protected override void OnSpawned()
        {
            enabled = isOwner;
        }

        private void Update()
        {
            bool isGrounded = IsGrounded();

            if (_lockedPosition)
            {
                transform.position = _lockedPosition.position;
                UpdateAnimatorParameters(true);
                return;
            }

            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                animator.SetBool(JumpHash, true);
            }
            else
            {
                animator.SetBool(JumpHash, false);
            }

            UpdateAnimatorParameters(isGrounded);
            
            if (isOnShipDeck)
            {
                return;
            }
            
            visualRoot.position = physicsRoot.position;
            visualRoot.rotation = physicsRoot.rotation;
        }

        private void UpdateAnimatorParameters(bool isGrounded)
        {
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

        public void SetLockedPosition([CanBeNull] Transform lockedPosition)
        {
            this._lockedPosition = lockedPosition;
            rb.isKinematic = lockedPosition != null;
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

        public void EnterShip()
        {
            isOnProxyShip = true;
        }

        public void ExitShip()
        {
            isOnProxyShip = false;
        }

        public void OnEnterShipProxy(Transform proxy, Transform realShip)
        {
            isOnProxyShip = true;
            isOnShipDeck = true;
        }

        public void OnExitShipProxy()
        {
            isOnShipDeck = false;
            isOnProxyShip = false;
            visualRoot.position = physicsRoot.position;
            visualRoot.rotation = physicsRoot.rotation;
        }
    }
}