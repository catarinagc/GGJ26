using UnityEngine;
using UnityEngine.UI;
using Combat;

namespace UI
{
    /// <summary>
    /// UI controller for displaying player health.
    /// Syncs with Health.cs events for real-time updates.
    /// </summary>
    public class PlayerHealthUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Health _playerHealth;

        [Header("Health Bar")]
        [SerializeField] private Image _healthBarFill;
        [SerializeField] private Text _healthText;

        [Header("Colors")]
        [SerializeField] private Color _healthyColor = new Color(0.2f, 0.8f, 0.2f, 1f);
        [SerializeField] private Color _damagedColor = new Color(0.9f, 0.7f, 0.1f, 1f);
        [SerializeField] private Color _criticalColor = new Color(0.9f, 0.2f, 0.2f, 1f);
        [SerializeField] private float _damagedThreshold = 0.5f;
        [SerializeField] private float _criticalThreshold = 0.3f;

        [Header("Animation")]
        [SerializeField] private float _smoothSpeed = 5f;
        [SerializeField] private bool _animateHealthChange = true;

        private float _targetFillAmount = 1f;
        private float _currentFillAmount = 1f;

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
        }

        private void OnEnable()
        {
            if (_playerHealth != null)
            {
                _playerHealth.OnHealthChanged += OnHealthChanged;
                // Initialize with current health
                OnHealthChanged(_playerHealth.CurrentHealth, _playerHealth.MaxHealth);
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
            // Initial update
            if (_playerHealth != null)
            {
                OnHealthChanged(_playerHealth.CurrentHealth, _playerHealth.MaxHealth);
                _currentFillAmount = _targetFillAmount;
                UpdateHealthBarVisual(_currentFillAmount);
            }
        }

        private void Update()
        {
            if (_animateHealthChange && Mathf.Abs(_currentFillAmount - _targetFillAmount) > 0.001f)
            {
                _currentFillAmount = Mathf.Lerp(_currentFillAmount, _targetFillAmount, Time.deltaTime * _smoothSpeed);
                UpdateHealthBarVisual(_currentFillAmount);
            }
        }

        private void OnHealthChanged(float currentHealth, float maxHealth)
        {
            float healthPercent = maxHealth > 0 ? currentHealth / maxHealth : 0f;
            _targetFillAmount = healthPercent;

            if (!_animateHealthChange)
            {
                _currentFillAmount = _targetFillAmount;
                UpdateHealthBarVisual(_currentFillAmount);
            }

            // Update text
            if (_healthText != null)
            {
                _healthText.text = $"{Mathf.CeilToInt(currentHealth)}/{Mathf.CeilToInt(maxHealth)}";
            }
        }

        private void UpdateHealthBarVisual(float fillAmount)
        {
            if (_healthBarFill != null)
            {
                _healthBarFill.fillAmount = fillAmount;
                _healthBarFill.color = GetHealthColor(fillAmount);
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
