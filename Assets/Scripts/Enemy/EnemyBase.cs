using UnityEngine;
using Combat;

namespace Enemy
{
    /// <summary>
    /// Base class for all enemies.
    /// Provides common functionality like health management and death handling.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class EnemyBase : MonoBehaviour
    {
        [Header("Enemy Settings")]
        [SerializeField] private string _enemyName = "Enemy";
        [SerializeField] private bool _destroyOnDeath = false;
        [SerializeField] private float _destroyDelay = 0f;

        [Header("Visual Feedback")]
        [SerializeField] private bool _flashOnHit = true;
        [SerializeField] private Color _hitFlashColor = Color.red;
        [SerializeField] private float _hitFlashDuration = 0.1f;

        // Components
        protected Health _health;
        protected SpriteRenderer _spriteRenderer;
        
        // State
        private Color _originalColor;
        private float _flashTimer;
        private bool _isFlashing;

        public string EnemyName => _enemyName;
        public Health Health => _health;
        public bool IsAlive => _health != null && _health.IsAlive;

        protected virtual void Awake()
        {
            _health = GetComponent<Health>();
            _spriteRenderer = GetComponent<SpriteRenderer>();

            if (_spriteRenderer != null)
            {
                _originalColor = _spriteRenderer.color;
            }
        }

        protected virtual void OnEnable()
        {
            if (_health != null)
            {
                _health.OnDamageTaken += HandleDamageTaken;
                _health.OnDeath += HandleDeath;
            }
        }

        protected virtual void OnDisable()
        {
            if (_health != null)
            {
                _health.OnDamageTaken -= HandleDamageTaken;
                _health.OnDeath -= HandleDeath;
            }
        }

        protected virtual void Update()
        {
            // Handle hit flash
            if (_isFlashing)
            {
                _flashTimer -= Time.deltaTime;
                if (_flashTimer <= 0)
                {
                    _isFlashing = false;
                    if (_spriteRenderer != null)
                    {
                        _spriteRenderer.color = _originalColor;
                    }
                }
            }
        }

        protected virtual void HandleDamageTaken(float damage)
        {
            Debug.Log($"[Enemy] {_enemyName} took {damage} damage! Remaining HP: {_health.CurrentHealth}/{_health.MaxHealth}");

            // Visual feedback
            if (_flashOnHit && _spriteRenderer != null)
            {
                _spriteRenderer.color = _hitFlashColor;
                _flashTimer = _hitFlashDuration;
                _isFlashing = true;
            }

            OnDamageTaken(damage);
        }

        protected virtual void HandleDeath()
        {
            Debug.Log($"[Enemy] {_enemyName} has been defeated!");

            OnDeath();

            if (_destroyOnDeath)
            {
                if (_destroyDelay > 0)
                {
                    Destroy(gameObject, _destroyDelay);
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }

        /// <summary>
        /// Override in derived classes for custom damage behavior.
        /// </summary>
        protected virtual void OnDamageTaken(float damage) { }

        /// <summary>
        /// Override in derived classes for custom death behavior.
        /// </summary>
        protected virtual void OnDeath() { }
    }
}
