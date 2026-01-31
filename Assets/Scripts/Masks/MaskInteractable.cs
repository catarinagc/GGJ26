using UnityEngine;
using UnityEngine.UI;

namespace Masks
{
    /// <summary>
    /// World object that allows the player to pick up or swap masks.
    /// Features floating animation and glow effect for visual appeal.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class MaskInteractable : MonoBehaviour
    {
        [Header("Mask Data")]
        [SerializeField] private MaskData _maskData;

        [Header("Interaction")]
        [SerializeField] private KeyCode _interactKey = KeyCode.E;
        [SerializeField] private string _interactButton = "MaskAbility";
        [SerializeField] private float _interactionRange = 2f;

        [Header("Floating Effect")]
        [SerializeField] private bool _enableFloating = true;
        [SerializeField] private float _floatAmplitude = 0.3f;
        [SerializeField] private float _floatFrequency = 1.5f;

        [Header("Rotation Effect")]
        [SerializeField] private bool _enableRotation = true;
        [SerializeField] private float _rotationSpeed = 30f;

        [Header("Glow Effect")]
        [SerializeField] private bool _enableGlow = true;
        [SerializeField] private float _glowPulseSpeed = 2f;
        [SerializeField] private float _glowMinIntensity = 0.5f;
        [SerializeField] private float _glowMaxIntensity = 1.2f;
        [SerializeField] private Color _glowColor = new Color(1f, 0.8f, 0.2f, 1f);

        [Header("UI Prompt")]
        [SerializeField] private GameObject _promptUI;
        [SerializeField] private Text _promptText;
        [SerializeField] private string _swapPromptFormat = "Press [E] to Swap: {0}";
        [SerializeField] private string _pickupPromptFormat = "Press [E] to Pick Up: {0}";
        [SerializeField] private string _alreadyEquippedText = "Already Equipped";

        [Header("Visual Components")]
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private SpriteRenderer _glowRenderer;
        [SerializeField] private ParticleSystem _pickupParticles;

        [Header("Audio")]
        [SerializeField] private AudioClip _pickupSound;
        [SerializeField] private AudioSource _audioSource;

        // Runtime state
        private Vector3 _startPosition;
        private float _floatTimer;
        private bool _playerInRange;
        private MaskManager _playerMaskManager;
        private Color _originalSpriteColor;
        private bool _isInteractable = true;

        public MaskData MaskData => _maskData;
        public bool IsPlayerInRange => _playerInRange;

        private void Awake()
        {
            _startPosition = transform.position;

            // Auto-setup components if not assigned
            if (_spriteRenderer == null)
                _spriteRenderer = GetComponent<SpriteRenderer>();

            if (_audioSource == null)
                _audioSource = GetComponent<AudioSource>();

            if (_spriteRenderer != null)
                _originalSpriteColor = _spriteRenderer.color;

            // Ensure collider is trigger
            var collider = GetComponent<Collider2D>();
            if (collider != null)
                collider.isTrigger = true;

            // Hide prompt initially
            if (_promptUI != null)
            {
                _promptUI.SetActive(false);
            }
            else
            {
                // Try to find it if not assigned
                var promptGO = GameObject.Find("Canvas/InteractionPrompt");
                if (promptGO != null)
                {
                    _promptUI = promptGO;
                    _promptText = promptGO.GetComponentInChildren<Text>();
                    _promptUI.SetActive(false);
                }
            }
        }

        private void Start()
        {
            // Setup visual based on mask data
            UpdateVisuals();
            
            // Double check UI is hidden
            if (_promptUI != null)
                _promptUI.SetActive(false);
        }

        private void Update()
        {
            // Visual effects
            if (_enableFloating)
                ApplyFloatingEffect();

            if (_enableRotation)
                ApplyRotationEffect();

            if (_enableGlow)
                ApplyGlowEffect();

            // Interaction check
            if (_playerInRange && _isInteractable)
            {
                CheckForInteraction();
            }
        }

        private void ApplyFloatingEffect()
        {
            _floatTimer += Time.deltaTime * _floatFrequency;
            float yOffset = Mathf.Sin(_floatTimer * Mathf.PI * 2f) * _floatAmplitude;
            transform.position = _startPosition + Vector3.up * yOffset;
        }

        private void ApplyRotationEffect()
        {
            transform.Rotate(Vector3.forward, _rotationSpeed * Time.deltaTime);
        }

        private void ApplyGlowEffect()
        {
            if (_glowRenderer != null)
            {
                float pulse = Mathf.Lerp(_glowMinIntensity, _glowMaxIntensity,
                    (Mathf.Sin(Time.time * _glowPulseSpeed) + 1f) * 0.5f);
                
                Color glowColor = _glowColor;
                glowColor.a = pulse;
                _glowRenderer.color = glowColor;
            }
            else if (_spriteRenderer != null)
            {
                // Apply glow to main sprite if no separate glow renderer
                float pulse = Mathf.Lerp(_glowMinIntensity, _glowMaxIntensity,
                    (Mathf.Sin(Time.time * _glowPulseSpeed) + 1f) * 0.5f);
                
                _spriteRenderer.color = _originalSpriteColor * pulse;
            }
        }

        private void CheckForInteraction()
        {
            // Check for E key or input action
            bool interactPressed = Input.GetKeyDown(_interactKey);

            if (interactPressed)
            {
                TryInteract();
            }
        }

        private void TryInteract()
        {
            if (_playerMaskManager == null || _maskData == null)
            {
                Debug.LogWarning("[MaskInteractable] Cannot interact: Missing MaskManager or MaskData.");
                return;
            }

            // Check if player already has this mask
            if (_playerMaskManager.CurrentMask == _maskData)
            {
                Debug.Log($"[MaskInteractable] Player already has {_maskData.MaskName} equipped.");
                return;
            }

            // Store the old mask (if any) to potentially spawn it
            MaskData oldMask = _playerMaskManager.CurrentMask;

            // Swap the mask
            _playerMaskManager.SwapMask(_maskData);

            Debug.Log($"[MaskInteractable] Player picked up: {_maskData.MaskName}");

            // Play effects
            PlayPickupEffects();

            // If player had a mask, we could spawn it here
            // For now, we'll just destroy this interactable or swap its data
            if (oldMask != null)
            {
                // Swap: Replace this interactable's mask with the old one
                _maskData = oldMask;
                UpdateVisuals();
                UpdatePromptText();
                Debug.Log($"[MaskInteractable] Dropped mask: {oldMask.MaskName}");
            }
            else
            {
                // No previous mask, destroy this pickup
                Destroy(gameObject, 0.1f);
                if (_promptUI != null) _promptUI.SetActive(false);
            }
        }

        private void PlayPickupEffects()
        {
            // Play particles
            if (_pickupParticles != null)
            {
                _pickupParticles.Play();
            }

            // Play sound
            if (_audioSource != null && _pickupSound != null)
            {
                _audioSource.PlayOneShot(_pickupSound);
            }
        }

        private void UpdateVisuals()
        {
            if (_maskData == null) return;

            // Update sprite if mask has an icon
            if (_spriteRenderer != null && _maskData.Icon != null)
            {
                _spriteRenderer.sprite = _maskData.Icon;
            }

            // Update glow color based on mask (could be customized per mask)
            // For now, use default glow color
        }

        private void UpdatePromptText()
        {
            if (_promptText == null || _maskData == null) return;

            if (_playerMaskManager == null)
            {
                _promptText.text = string.Format(_pickupPromptFormat, _maskData.MaskName);
                return;
            }

            // Check if player already has this mask
            if (_playerMaskManager.CurrentMask == _maskData)
            {
                _promptText.text = _alreadyEquippedText;
                _isInteractable = false;
            }
            else if (_playerMaskManager.CurrentMask != null)
            {
                // Player has a different mask - show swap prompt
                _promptText.text = string.Format(_swapPromptFormat, _maskData.MaskName);
                _isInteractable = true;
            }
            else
            {
                // Player has no mask - show pickup prompt
                _promptText.text = string.Format(_pickupPromptFormat, _maskData.MaskName);
                _isInteractable = true;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Check if it's the player
            MaskManager maskManager = other.GetComponent<MaskManager>();
            if (maskManager != null)
            {
                _playerInRange = true;
                _playerMaskManager = maskManager;

                // Show prompt
                if (_promptUI != null)
                {
                    _promptUI.SetActive(true);
                    UpdatePromptText();
                }

                Debug.Log($"[MaskInteractable] Player entered range of {_maskData?.MaskName ?? "Unknown Mask"}");
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            MaskManager maskManager = other.GetComponent<MaskManager>();
            if (maskManager != null && maskManager == _playerMaskManager)
            {
                _playerInRange = false;
                _playerMaskManager = null;

                // Hide prompt
                if (_promptUI != null)
                {
                    _promptUI.SetActive(false);
                }

                Debug.Log($"[MaskInteractable] Player left range of {_maskData?.MaskName ?? "Unknown Mask"}");
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Draw interaction range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _interactionRange);
        }

        /// <summary>
        /// Sets the mask data for this interactable (useful for runtime spawning).
        /// </summary>
        public void SetMaskData(MaskData maskData)
        {
            _maskData = maskData;
            UpdateVisuals();
        }

        /// <summary>
        /// Resets the floating position (call after moving the object).
        /// </summary>
        public void ResetFloatPosition()
        {
            _startPosition = transform.position;
            _floatTimer = 0f;
        }
    }
}
