using UnityEngine;
using UnityEngine.UI;
using Combat;
using Boss;

namespace UI
{
    /// <summary>
    /// UI controller for displaying boss health.
    /// Creates an ornate health bar at the bottom center of the screen.
    /// Syncs with Health.cs events for real-time updates.
    /// </summary>
    public class BossHealthUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Health _bossHealth;
        [SerializeField] private BossCountArmand _boss;

        [Header("Health Bar")]
        [SerializeField] private Slider _healthSlider;
        [SerializeField] private Image _healthFill;
        [SerializeField] private Text _bossNameText;
        [SerializeField] private Text _healthText;

        [Header("Colors")]
        [SerializeField] private Color _healthyColor = new Color(0.8f, 0.1f, 0.1f, 1f);
        [SerializeField] private Color _damagedColor = new Color(0.9f, 0.5f, 0.1f, 1f);
        [SerializeField] private Color _criticalColor = new Color(0.5f, 0.1f, 0.1f, 1f);
        [SerializeField] private float _damagedThreshold = 0.5f;
        [SerializeField] private float _criticalThreshold = 0.25f;

        [Header("Animation")]
        [SerializeField] private float _smoothSpeed = 3f;
        [SerializeField] private bool _animateHealthChange = true;
        [SerializeField] private float _shakeIntensity = 5f;
        [SerializeField] private float _shakeDuration = 0.2f;

        [Header("Visibility")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _fadeSpeed = 2f;
        [SerializeField] private bool _hideWhenFull = false;
        [SerializeField] private bool _hideWhenDefeated = true;

        private float _targetValue = 1f;
        private float _currentValue = 1f;
        private float _shakeTimer;
        private Vector3 _originalPosition;
        private RectTransform _rectTransform;
        private bool _isVisible = true;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            if (_rectTransform != null)
            {
                _originalPosition = _rectTransform.anchoredPosition;
            }

            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
            }
        }

        private void Start()
        {
            // Auto-find boss if not assigned
            if (_bossHealth == null || _boss == null)
            {
                FindBoss();
            }

            // Initialize
            if (_bossHealth != null)
            {
                OnHealthChanged(_bossHealth.CurrentHealth, _bossHealth.MaxHealth);
                _currentValue = _targetValue;
                UpdateHealthBarVisual(_currentValue);
            }

            // Set initial visibility
            if (_hideWhenFull && _bossHealth != null && _bossHealth.HealthPercentage >= 1f)
            {
                SetVisible(false, true);
            }
        }

        private void OnEnable()
        {
            if (_bossHealth != null)
            {
                _bossHealth.OnHealthChanged += OnHealthChanged;
                _bossHealth.OnDamageTaken += OnDamageTaken;
                _bossHealth.OnDeath += OnBossDeath;
            }

            if (_boss != null)
            {
                _boss.OnBossDefeated += OnBossDefeated;
            }
        }

        private void OnDisable()
        {
            if (_bossHealth != null)
            {
                _bossHealth.OnHealthChanged -= OnHealthChanged;
                _bossHealth.OnDamageTaken -= OnDamageTaken;
                _bossHealth.OnDeath -= OnBossDeath;
            }

            if (_boss != null)
            {
                _boss.OnBossDefeated -= OnBossDefeated;
            }
        }

        private void Update()
        {
            // Animate health bar
            if (_animateHealthChange && Mathf.Abs(_currentValue - _targetValue) > 0.001f)
            {
                _currentValue = Mathf.Lerp(_currentValue, _targetValue, Time.deltaTime * _smoothSpeed);
                UpdateHealthBarVisual(_currentValue);
            }

            // Handle shake effect
            if (_shakeTimer > 0)
            {
                _shakeTimer -= Time.deltaTime;
                if (_rectTransform != null)
                {
                    Vector2 shakeOffset = Random.insideUnitCircle * _shakeIntensity * (_shakeTimer / _shakeDuration);
                    _rectTransform.anchoredPosition = _originalPosition + (Vector3)shakeOffset;
                }
            }
            else if (_rectTransform != null && _rectTransform.anchoredPosition != (Vector2)_originalPosition)
            {
                _rectTransform.anchoredPosition = _originalPosition;
            }

            // Handle fade
            if (_canvasGroup != null)
            {
                float targetAlpha = _isVisible ? 1f : 0f;
                if (Mathf.Abs(_canvasGroup.alpha - targetAlpha) > 0.01f)
                {
                    _canvasGroup.alpha = Mathf.Lerp(_canvasGroup.alpha, targetAlpha, Time.deltaTime * _fadeSpeed);
                }
            }
        }

        private void FindBoss()
        {
            // Try to find by tag first
            GameObject bossObj = GameObject.FindGameObjectWithTag("Boss");
            if (bossObj == null)
            {
                // Try to find by name
                bossObj = GameObject.Find("Boss_CountArmand");
            }

            if (bossObj != null)
            {
                _bossHealth = bossObj.GetComponent<Health>();
                _boss = bossObj.GetComponent<BossCountArmand>();

                // Subscribe to events
                if (_bossHealth != null)
                {
                    _bossHealth.OnHealthChanged += OnHealthChanged;
                    _bossHealth.OnDamageTaken += OnDamageTaken;
                    _bossHealth.OnDeath += OnBossDeath;
                }

                if (_boss != null)
                {
                    _boss.OnBossDefeated += OnBossDefeated;
                }
            }
            else
            {
                Debug.LogWarning("[BossHealthUI] Could not find boss in scene!");
            }
        }

        private void OnHealthChanged(float currentHealth, float maxHealth)
        {
            float healthPercent = maxHealth > 0 ? currentHealth / maxHealth : 0f;
            _targetValue = healthPercent;

            if (!_animateHealthChange)
            {
                _currentValue = _targetValue;
                UpdateHealthBarVisual(_currentValue);
            }

            // Update text
            if (_healthText != null)
            {
                _healthText.text = $"{Mathf.CeilToInt(currentHealth)}/{Mathf.CeilToInt(maxHealth)}";
            }

            // Show health bar when damaged
            if (_hideWhenFull && healthPercent < 1f)
            {
                SetVisible(true);
            }
        }

        private void OnDamageTaken(float damage)
        {
            // Trigger shake effect
            _shakeTimer = _shakeDuration;
        }

        private void OnBossDeath()
        {
            if (_hideWhenDefeated)
            {
                SetVisible(false);
            }
        }

        private void OnBossDefeated()
        {
            if (_hideWhenDefeated)
            {
                SetVisible(false);
            }
        }

        private void UpdateHealthBarVisual(float value)
        {
            if (_healthSlider != null)
            {
                _healthSlider.value = value;
            }

            if (_healthFill != null)
            {
                _healthFill.color = GetHealthColor(value);
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

        public void SetVisible(bool visible, bool instant = false)
        {
            _isVisible = visible;

            if (instant && _canvasGroup != null)
            {
                _canvasGroup.alpha = visible ? 1f : 0f;
            }
        }

        /// <summary>
        /// Manually set the boss health reference.
        /// </summary>
        public void SetBossHealth(Health health)
        {
            // Unsubscribe from old
            if (_bossHealth != null)
            {
                _bossHealth.OnHealthChanged -= OnHealthChanged;
                _bossHealth.OnDamageTaken -= OnDamageTaken;
                _bossHealth.OnDeath -= OnBossDeath;
            }

            _bossHealth = health;

            // Subscribe to new
            if (_bossHealth != null)
            {
                _bossHealth.OnHealthChanged += OnHealthChanged;
                _bossHealth.OnDamageTaken += OnDamageTaken;
                _bossHealth.OnDeath += OnBossDeath;
                OnHealthChanged(_bossHealth.CurrentHealth, _bossHealth.MaxHealth);
            }
        }

        /// <summary>
        /// Set the boss name displayed above the health bar.
        /// </summary>
        public void SetBossName(string name)
        {
            if (_bossNameText != null)
            {
                _bossNameText.text = name;
            }
        }
    }
}
