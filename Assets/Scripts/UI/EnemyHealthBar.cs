using UnityEngine;
using UnityEngine.UI;
using Combat;

namespace UI
{
    /// <summary>
    /// World-space floating health bar for enemies.
    /// Appears above the enemy and updates based on Health.cs events.
    /// </summary>
    public class EnemyHealthBar : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Image _healthBarFill;
        [SerializeField] private Image _healthBarBackground;
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("Settings")]
        [SerializeField] private Vector3 _offset = new Vector3(0f, 1.5f, 0f);
        [SerializeField] private bool _hideWhenFull = true;
        [SerializeField] private float _showDuration = 3f;
        [SerializeField] private float _fadeSpeed = 2f;

        [Header("Colors")]
        [SerializeField] private Color _healthyColor = new Color(0.2f, 0.8f, 0.2f, 1f);
        [SerializeField] private Color _damagedColor = new Color(0.9f, 0.7f, 0.1f, 1f);
        [SerializeField] private Color _criticalColor = new Color(0.9f, 0.2f, 0.2f, 1f);
        [SerializeField] private float _damagedThreshold = 0.5f;
        [SerializeField] private float _criticalThreshold = 0.3f;

        [Header("Billboard")]
        [SerializeField] private bool _faceCamera = true;

        private Health _health;
        private Transform _target;
        private Camera _mainCamera;
        private float _showTimer;
        private float _targetAlpha;
        private bool _isInitialized;

        private void Awake()
        {
            _mainCamera = Camera.main;
            
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
            }

            // Start hidden
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
            }
        }

        private void OnEnable()
        {
            if (_health != null)
            {
                _health.OnHealthChanged += OnHealthChanged;
                _health.OnDeath += OnDeath;
            }
        }

        private void OnDisable()
        {
            if (_health != null)
            {
                _health.OnHealthChanged -= OnHealthChanged;
                _health.OnDeath -= OnDeath;
            }
        }

        private void LateUpdate()
        {
            // Follow target
            if (_target != null)
            {
                transform.position = _target.position + _offset;
            }

            // Billboard effect
            if (_faceCamera && _mainCamera != null)
            {
                transform.rotation = _mainCamera.transform.rotation;
            }

            // Handle visibility timer
            if (_showTimer > 0)
            {
                _showTimer -= Time.deltaTime;
                if (_showTimer <= 0 && _hideWhenFull && _health != null && _health.HealthPercentage >= 1f)
                {
                    _targetAlpha = 0f;
                }
            }

            // Fade alpha
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = Mathf.MoveTowards(_canvasGroup.alpha, _targetAlpha, Time.deltaTime * _fadeSpeed);
            }
        }

        /// <summary>
        /// Initialize the health bar with a target Health component.
        /// </summary>
        public void Initialize(Health health, Transform target)
        {
            _health = health;
            _target = target;

            if (_health != null)
            {
                _health.OnHealthChanged += OnHealthChanged;
                _health.OnDeath += OnDeath;
                
                // Initial update
                UpdateHealthBar(_health.CurrentHealth, _health.MaxHealth);
            }

            _isInitialized = true;
        }

        private void OnHealthChanged(float currentHealth, float maxHealth)
        {
            UpdateHealthBar(currentHealth, maxHealth);
            
            // Show the health bar when damaged
            _showTimer = _showDuration;
            _targetAlpha = 1f;
        }

        private void UpdateHealthBar(float currentHealth, float maxHealth)
        {
            float healthPercent = maxHealth > 0 ? currentHealth / maxHealth : 0f;

            if (_healthBarFill != null)
            {
                _healthBarFill.fillAmount = healthPercent;
                _healthBarFill.color = GetHealthColor(healthPercent);
            }
        }

        private Color GetHealthColor(float healthPercent)
        {
            if (healthPercent <= _criticalThreshold)
            {
                return _criticalColor;
            }
            else if (healthPercent <= _damagedThreshold)
            {
                return _damagedColor;
            }
            return _healthyColor;
        }

        private void OnDeath()
        {
            // Fade out and destroy
            _targetAlpha = 0f;
            Destroy(gameObject, 1f);
        }

        private void OnDestroy()
        {
            if (_health != null)
            {
                _health.OnHealthChanged -= OnHealthChanged;
                _health.OnDeath -= OnDeath;
            }
        }
    }
}
