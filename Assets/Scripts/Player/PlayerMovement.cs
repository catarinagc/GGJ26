using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovement : MonoBehaviour, IMovement
    {
        [Header("Movement")]
        [SerializeField] private float _maxSpeed = 12f;
        [SerializeField] private float _acceleration = 60f;
        [SerializeField] private float _deceleration = 60f;
        [SerializeField] private float _airControlMultiplier = 0.8f;

        [Header("Jump")]
        [SerializeField] private float _jumpForce = 16f;
        [SerializeField] private float _jumpCutMultiplier = 0.5f;
        [SerializeField] private float _gravityScale = 3f;
        [SerializeField] private float _fallGravityMultiplier = 1.5f;

        [Header("Dash")]
        [SerializeField] private float _dashSpeed = 25f;
        [SerializeField] private float _dashDuration = 0.15f;

        [Header("Ground Detection")]
        [SerializeField] private LayerMask _groundLayer;
        [SerializeField] private Transform _groundCheck;
        [SerializeField] private Vector2 _groundCheckSize = new Vector2(0.5f, 0.1f);

        private Rigidbody2D _rb;
        private Vector2 _moveInput;
        private bool _isDashing;
        private float _dashTimeLeft;
        private Vector2 _dashDirection;
        private bool _facingRight = true;

        public bool IsGrounded { get; private set; }
        public Vector2 Velocity => _rb.linearVelocity;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = _gravityScale;
        }

        private void FixedUpdate()
        {
            CheckGround();

            if (_isDashing)
            {
                HandleDash();
            }
            else
            {
                HandleMovement();
                HandleGravity();
            }
        }

        private void CheckGround()
        {
            if (_groundCheck == null)
            {
                // Fallback if ground check transform is missing, check relative to transform
                IsGrounded = Physics2D.OverlapBox((Vector2)transform.position + Vector2.down * 0.5f, _groundCheckSize, 0f, _groundLayer);
            }
            else
            {
                IsGrounded = Physics2D.OverlapBox(_groundCheck.position, _groundCheckSize, 0f, _groundLayer);
            }
        }

        private void HandleMovement()
        {
            // Calculate target speed
            float targetSpeed = _moveInput.x * _maxSpeed;
            
            // Calculate acceleration rate
            float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? _acceleration : _deceleration;
            
            // Apply air control
            if (!IsGrounded)
            {
                accelRate *= _airControlMultiplier;
            }

            // Apply movement force/velocity
            float newX = Mathf.MoveTowards(_rb.linearVelocity.x, targetSpeed, accelRate * Time.fixedDeltaTime);
            _rb.linearVelocity = new Vector2(newX, _rb.linearVelocity.y);

            // Handle facing direction
            if (_moveInput.x > 0 && !_facingRight)
            {
                Flip();
            }
            else if (_moveInput.x < 0 && _facingRight)
            {
                Flip();
            }
        }

        private void Flip()
        {
            _facingRight = !_facingRight;
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }

        private void HandleGravity()
        {
            // Apply higher gravity when falling for better feel
            if (_rb.linearVelocity.y < 0)
            {
                _rb.gravityScale = _gravityScale * _fallGravityMultiplier;
            }
            else
            {
                _rb.gravityScale = _gravityScale;
            }
        }

        private void HandleDash()
        {
            _rb.linearVelocity = _dashDirection * _dashSpeed;
            _dashTimeLeft -= Time.fixedDeltaTime;

            if (_dashTimeLeft <= 0)
            {
                EndDash();
            }
        }

        private void EndDash()
        {
            _isDashing = false;
            // Retain some momentum but clamp to max speed
            Vector2 exitVelocity = _dashDirection.normalized * _maxSpeed;
            // Only apply x velocity, preserve y if needed or reset it?
            // Usually resetting y is safer to avoid super jumps if dashing up.
            // But if dashing horizontally, y should be 0 (gravity will take over).
            _rb.linearVelocity = new Vector2(exitVelocity.x, _rb.linearVelocity.y > 0 ? _rb.linearVelocity.y : 0);
        }

        public void SetMoveInput(Vector2 direction)
        {
            _moveInput = direction;
        }

        public void Jump()
        {
            // Reset vertical velocity for consistent jump height
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, 0);
            _rb.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
        }

        public void CutJump()
        {
            if (_rb.linearVelocity.y > 0)
            {
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _rb.linearVelocity.y * _jumpCutMultiplier);
            }
        }

        public void Dash(Vector2 direction)
        {
            if (_isDashing) return;

            // Determine dash direction
            if (direction.sqrMagnitude < 0.1f)
            {
                // Default to facing direction if no input
                direction = new Vector2(_facingRight ? 1 : -1, 0);
            }
            else
            {
                // Snap to 8 directions or keep free? 
                // Let's keep it free for now, but normalize.
                direction.Normalize();
            }
            
            _isDashing = true;
            _dashTimeLeft = _dashDuration;
            _dashDirection = direction;
            
            // Reset velocity for instant dash
            _rb.linearVelocity = Vector2.zero;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            if (_groundCheck != null)
            {
                Gizmos.DrawWireCube(_groundCheck.position, _groundCheckSize);
            }
            else
            {
                Gizmos.DrawWireCube((Vector2)transform.position + Vector2.down * 0.5f, _groundCheckSize);
            }
        }
    }
}
