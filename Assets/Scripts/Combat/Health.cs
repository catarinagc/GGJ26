using System;
using UnityEngine;

namespace Combat
{
    /// <summary>
    /// Health component that implements IDamageable.
    /// Attach to any object that can take damage.
    /// </summary>
    public class Health : MonoBehaviour, IDamageable
    {
        [Header("Health Settings")]
        [SerializeField] private float _maxHealth = 100f;
        [SerializeField] private float _currentHealth;

        [Header("Knockback Settings")]
        [SerializeField] private bool _applyKnockback = true;
        [SerializeField] private float _knockbackResistance = 0f;

        [Header("Invincibility")]
        [SerializeField] private float _invincibilityDuration = 0f;
        [SerializeField] private bool _isInvincible;

        [Header("Debug")]
        [SerializeField] private bool _logDamage = true;

        // Events
        public event Action<float, float> OnHealthChanged; // currentHealth, maxHealth
        public event Action<float> OnDamageTaken; // damage amount
        public event Action OnDeath;

        // Properties
        public float MaxHealth => _maxHealth;
        public float CurrentHealth => _currentHealth;
        public bool IsAlive => _currentHealth > 0;
        public bool IsInvincible => _isInvincible;
        public float HealthPercentage => _maxHealth > 0 ? _currentHealth / _maxHealth : 0f;

        private Rigidbody2D _rb;
        private float _invincibilityTimer;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _currentHealth = _maxHealth;
        }

        private void Update()
        {
            // Handle invincibility timer
            if (_isInvincible && _invincibilityDuration > 0)
            {
                _invincibilityTimer -= Time.deltaTime;
                if (_invincibilityTimer <= 0)
                {
                    _isInvincible = false;
                }
            }
        }

        /// <summary>
        /// Apply damage to this object.
        /// </summary>
        public void TakeDamage(float damage, Vector2 knockbackDirection, float knockbackForce)
        {
            if (!IsAlive) return;
            if (_isInvincible) return;

            // Apply damage
            float previousHealth = _currentHealth;
            _currentHealth = Mathf.Max(0, _currentHealth - damage);

            if (_logDamage)
            {
                Debug.Log($"[Health] {gameObject.name} took {damage} damage. Health: {previousHealth} -> {_currentHealth}/{_maxHealth}");
            }

            // Fire events
            OnDamageTaken?.Invoke(damage);
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);

            // Apply knockback
            if (_applyKnockback && _rb != null && knockbackForce > 0)
            {
                float actualKnockback = Mathf.Max(0, knockbackForce - _knockbackResistance);
                _rb.AddForce(knockbackDirection.normalized * actualKnockback, ForceMode2D.Impulse);
            }

            // Start invincibility
            if (_invincibilityDuration > 0)
            {
                _isInvincible = true;
                _invincibilityTimer = _invincibilityDuration;
            }

            // Check for death
            if (_currentHealth <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// Heal the object by the specified amount.
        /// </summary>
        public void Heal(float amount)
        {
            if (!IsAlive) return;

            float previousHealth = _currentHealth;
            _currentHealth = Mathf.Min(_maxHealth, _currentHealth + amount);

            if (_logDamage)
            {
                Debug.Log($"[Health] {gameObject.name} healed {amount}. Health: {previousHealth} -> {_currentHealth}/{_maxHealth}");
            }

            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        }

        /// <summary>
        /// Set health to a specific value.
        /// </summary>
        public void SetHealth(float value)
        {
            _currentHealth = Mathf.Clamp(value, 0, _maxHealth);
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);

            if (_currentHealth <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// Reset health to max.
        /// </summary>
        public void ResetHealth()
        {
            _currentHealth = _maxHealth;
            _isInvincible = false;
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        }

        /// <summary>
        /// Set max health and optionally reset current health.
        /// </summary>
        public void SetMaxHealth(float value, bool resetCurrent = false)
        {
            _maxHealth = Mathf.Max(1, value);
            if (resetCurrent)
            {
                _currentHealth = _maxHealth;
            }
            else
            {
                _currentHealth = Mathf.Min(_currentHealth, _maxHealth);
            }
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        }

        private void Die()
        {
            if (_logDamage)
            {
                Debug.Log($"[Health] {gameObject.name} has died!");
            }

            OnDeath?.Invoke();
        }

        private void OnValidate()
        {
            // Ensure current health doesn't exceed max in editor
            _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
        }
    }
}
