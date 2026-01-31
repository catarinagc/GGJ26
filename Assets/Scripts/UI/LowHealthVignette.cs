using UnityEngine;
using UnityEngine.UI;
using Combat;

namespace UI
{
    /// <summary>
    /// Red vignette effect that fades in when player health is low.
    /// Creates a transparent red border around the screen.
    /// </summary>
    public class LowHealthVignette : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Health _playerHealth;
        [SerializeField] private Image _vignetteImage;

        [Header("Settings")]
        [SerializeField] private float _healthThreshold = 0.3f;
        [SerializeField] private float _maxAlpha = 0.5f;
        [SerializeField] private float _fadeSpeed = 3f;
        [SerializeField] private bool _pulseEffect = true;
        [SerializeField] private float _pulseSpeed = 2f;
        [SerializeField] private float _pulseIntensity = 0.2f;

        [Header("Color")]
        [SerializeField] private Color _vignetteColor = new Color(0.8f, 0f, 0f, 1f);

        private float _targetAlpha;
        private float _currentAlpha;
        private float _pulseTimer;

        private void Awake()
        {
            // Auto-find player health if not assigned
            if (_playerHealth == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player == null)
                {
                    player = GameObject.Find("Player");
                }
                
                if (player != null)
                {
                    _playerHealth = player.GetComponent<Health>();
                }
            }

            // Initialize vignette as invisible
            if (_vignetteImage != null)
            {
                Color c = _vignetteColor;
                c.a = 0f;
                _vignetteImage.color = c;
            }
        }

        private void OnEnable()
        {
            if (_playerHealth != null)
            {
                _playerHealth.OnHealthChanged += OnHealthChanged;
            }
        }

        private void OnDisable()
        {
            if (_playerHealth != null)
            {
                _playerHealth.OnHealthChanged -= OnHealthChanged;
            }
        }

        private void Start()
        {
            // Initial check
            if (_playerHealth != null)
            {
                OnHealthChanged(_playerHealth.CurrentHealth, _playerHealth.MaxHealth);
            }
        }

        private void Update()
        {
            // Pulse effect when low health
            if (_pulseEffect && _targetAlpha > 0)
            {
                _pulseTimer += Time.deltaTime * _pulseSpeed;
                float pulse = Mathf.Sin(_pulseTimer) * _pulseIntensity;
                _currentAlpha = Mathf.Lerp(_currentAlpha, _targetAlpha + pulse, Time.deltaTime * _fadeSpeed);
            }
            else
            {
                _currentAlpha = Mathf.Lerp(_currentAlpha, _targetAlpha, Time.deltaTime * _fadeSpeed);
            }

            // Update vignette alpha
            if (_vignetteImage != null)
            {
                Color c = _vignetteColor;
                c.a = Mathf.Clamp01(_currentAlpha);
                _vignetteImage.color = c;
            }
        }

        private void OnHealthChanged(float currentHealth, float maxHealth)
        {
            float healthPercent = maxHealth > 0 ? currentHealth / maxHealth : 1f;

            if (healthPercent <= _healthThreshold)
            {
                // Calculate intensity based on how low health is
                float intensity = 1f - (healthPercent / _healthThreshold);
                _targetAlpha = intensity * _maxAlpha;
            }
            else
            {
                _targetAlpha = 0f;
            }
        }

        /// <summary>
        /// Manually set the player health reference.
        /// </summary>
        public void SetPlayerHealth(Health health)
        {
            if (_playerHealth != null)
            {
                _playerHealth.OnHealthChanged -= OnHealthChanged;
            }

            _playerHealth = health;

            if (_playerHealth != null)
            {
                _playerHealth.OnHealthChanged += OnHealthChanged;
                OnHealthChanged(_playerHealth.CurrentHealth, _playerHealth.MaxHealth);
            }
        }
    }
}
