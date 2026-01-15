using PurrNet;
using UnityEngine;

namespace BallTest
{
    public class PlayerMovement : NetworkIdentity
    {
        [SerializeField] private Rigidbody rb;
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float jumpForce = 5f;

        [SerializeField] private SyncInput<Vector2> moveInput = new();
        [SerializeField] private SyncInput<bool> jumpInput = new();

        private bool _jump;

        private void Awake()
        {
            jumpInput.onChanged += OnJump;
            jumpInput.onSentData += OnSentData;
        }

        protected override void OnSpawned()
        {
            if (isOwner)
            {
                PlayerCamera.Instance.SetTarget(rb.transform);
            }
        }

        protected override void OnDestroy()
        {
            jumpInput.onChanged -= OnJump;
            jumpInput.onSentData -= OnSentData;
        }

        private void OnSentData()
        {
            _jump = false;
        }

        private void OnJump(bool newInput)
        {
            if (newInput)
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        private void Update()
        {
            if (!isOwner)
                return;

            moveInput.value = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

            if (!_jump)
                _jump = Input.GetKeyDown(KeyCode.Space);
            jumpInput.value = _jump;
        }

        private void FixedUpdate()
        {
            if (!isServer)
                return;

            Vector3 move = new Vector3(moveInput.value.x, 0, moveInput.value.y).normalized;
            rb.AddForce(move * moveSpeed);
        }

        private void OnCollisionEnter(Collision other)
        {
            if (!isServer) return;

            if (other.transform.TryGetComponent(out PlayerHealth otherPlayer))
            {
                otherPlayer.HitOtherPlayer();

                var dir = (transform.position - otherPlayer.transform.position).normalized;
                rb.AddForce(dir * 4f, ForceMode.Impulse);
            }
        }
    }
}