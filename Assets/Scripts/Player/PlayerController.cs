using UnityEngine;
using UnityEngine.InputSystem;
using Masks;

namespace Player
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Input Actions")]
        [SerializeField] private InputActionAsset _inputActionAsset;
        [SerializeField] private string _actionMapName = "Player";
        [SerializeField] private string _moveActionName = "Move";
        [SerializeField] private string _jumpActionName = "Jump";
        [SerializeField] private string _dashActionName = "Dash";
        [SerializeField] private string _maskAbilityActionName = "MaskAbility";

        private InputAction _moveAction;
        private InputAction _jumpAction;
        private InputAction _dashAction;
        private InputAction _maskAbilityAction;

        [Header("Jump Settings")]
        [SerializeField] private float _coyoteTime = 0.1f;
        [SerializeField] private float _jumpBufferTime = 0.1f;

        [Header("Dash Settings")]
        [SerializeField] private float _dashCooldown = 1.2f;

        [Header("Mask System")]
        [SerializeField] private MaskManager _maskManager;

        private IMovement _movement;
        private float _coyoteTimeCounter;
        private float _jumpBufferCounter;
        private float _dashCooldownCounter;
        private bool _isJumping;

        private void Awake()
        {
            _movement = GetComponent<IMovement>();
            if (_movement == null)
            {
                Debug.LogError("IMovement component missing from PlayerController!");
            }

            // Auto-find MaskManager if not assigned
            if (_maskManager == null)
            {
                _maskManager = GetComponent<MaskManager>();
                if (_maskManager == null)
                {
                    _maskManager = FindAnyObjectByType<MaskManager>();
                }
            }

            if (_inputActionAsset != null)
            {
                var map = _inputActionAsset.FindActionMap(_actionMapName);
                if (map != null)
                {
                    _moveAction = map.FindAction(_moveActionName);
                    _jumpAction = map.FindAction(_jumpActionName);
                    _dashAction = map.FindAction(_dashActionName);
                    _maskAbilityAction = map.FindAction(_maskAbilityActionName);
                }
                else
                {
                    Debug.LogError($"Action Map '{_actionMapName}' not found in Input Action Asset.");
                }
            }
            else
            {
                Debug.LogError("Input Action Asset not assigned to PlayerController!");
            }
        }

        private void OnEnable()
        {
            if (_jumpAction != null)
            {
                _jumpAction.Enable();
                _jumpAction.performed += OnJumpPerformed;
                _jumpAction.canceled += OnJumpCanceled;
            }
            if (_dashAction != null)
            {
                _dashAction.Enable();
                _dashAction.performed += OnDashPerformed;
            }
            if (_moveAction != null)
            {
                _moveAction.Enable();
            }
            if (_maskAbilityAction != null)
            {
                _maskAbilityAction.Enable();
                _maskAbilityAction.performed += OnMaskAbilityPerformed;
            }
        }

        private void OnDisable()
        {
            if (_jumpAction != null)
            {
                _jumpAction.performed -= OnJumpPerformed;
                _jumpAction.canceled -= OnJumpCanceled;
                _jumpAction.Disable();
            }
            if (_dashAction != null)
            {
                _dashAction.performed -= OnDashPerformed;
                _dashAction.Disable();
            }
            if (_moveAction != null)
            {
                _moveAction.Disable();
            }
            if (_maskAbilityAction != null)
            {
                _maskAbilityAction.performed -= OnMaskAbilityPerformed;
                _maskAbilityAction.Disable();
            }
        }

        private void Update()
        {
            if (_movement == null) return;

            HandleInput();
            UpdateTimers();
            CheckJumpBuffer();
        }

        private void HandleInput()
        {
            Vector2 moveInput = _moveAction != null ? _moveAction.ReadValue<Vector2>() : Vector2.zero;
            _movement.SetMoveInput(moveInput);
        }

        private void UpdateTimers()
        {
            // Coyote Time
            if (_movement.IsGrounded)
            {
                _coyoteTimeCounter = _coyoteTime;
                _isJumping = false;
            }
            else
            {
                _coyoteTimeCounter -= Time.deltaTime;
            }

            // Jump Buffer
            if (_jumpBufferCounter > 0)
            {
                _jumpBufferCounter -= Time.deltaTime;
            }

            // Dash Cooldown
            if (_dashCooldownCounter > 0)
            {
                _dashCooldownCounter -= Time.deltaTime;
            }
        }

        private void OnJumpPerformed(InputAction.CallbackContext context)
        {
            _jumpBufferCounter = _jumpBufferTime;
        }

        private void OnJumpCanceled(InputAction.CallbackContext context)
        {
            if (_isJumping && _movement.Velocity.y > 0)
            {
                _movement.CutJump();
            }
        }

        private void CheckJumpBuffer()
        {
            if (_jumpBufferCounter > 0 && _coyoteTimeCounter > 0)
            {
                _movement.Jump();
                _jumpBufferCounter = 0;
                _coyoteTimeCounter = 0; // Consume coyote time
                _isJumping = true;
            }
        }

        private void OnDashPerformed(InputAction.CallbackContext context)
        {
            if (_dashCooldownCounter <= 0)
            {
                Vector2 dashDirection = _moveAction != null ? _moveAction.ReadValue<Vector2>() : Vector2.zero;
                _movement.Dash(dashDirection);
                _dashCooldownCounter = _dashCooldown;
            }
        }

        private void OnMaskAbilityPerformed(InputAction.CallbackContext context)
        {
            if (_maskManager != null)
            {
                _maskManager.TriggerMaskAbility();
            }
            else
            {
                Debug.LogWarning("[PlayerController] MaskManager not found. Cannot trigger mask ability.");
            }
        }

        /// <summary>
        /// Sets the MaskManager reference (for dependency injection or runtime assignment).
        /// </summary>
        public void SetMaskManager(MaskManager maskManager)
        {
            _maskManager = maskManager;
        }
    }
}
