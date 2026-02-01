using UnityEngine;
using Combat;

namespace Boss
{
    /// <summary>
    /// Projectile used by boss attacks. Similar to regular Projectile but configured
    /// for boss-specific behavior and uses the BossProjectile layer.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class BossProjectile : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _speed = 10f;
        [SerializeField] private float _damage = 10f;
        [SerializeField] private float _knockbackForce = 5f;
        [SerializeField] private float _lifetime = 5f;
        [SerializeField] private LayerMask _targetLayers;
        [SerializeField] private LayerMask _obstacleLayer;

        [Header("Visual")]
        [SerializeField] private bool _rotateToDirection = true;
        [SerializeField] private TrailRenderer _trailRenderer;
        [SerializeField] private ParticleSystem _hitParticles;

        private Rigidbody2D _rb;
        private Vector2 _direction;
        private bool _initialized;
        private AudioManager _audioManager;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.bodyType = RigidbodyType2D.Kinematic;

            // Ensure collider is trigger
            Collider2D col = GetComponent<Collider2D>();
            if (col != null)
            {
                col.isTrigger = true;
            }

            // Set layer to BossProjectile (layer 10)
            gameObject.layer = 10;

            // Try to find audio manager
            GameObject audioObj = GameObject.FindGameObjectWithTag("AudioManager");
            if (audioObj != null)
            {
                _audioManager = audioObj.GetComponent<AudioManager>();
            }
        }

        /// <summary>
        /// Initialize the projectile with direction and settings.
        /// </summary>
        public void Initialize(Vector2 direction, float speed, float damage, float knockbackForce, float lifetime, LayerMask targetLayers)
        {
            _direction = direction.normalized;
            _speed = speed;
            _damage = damage;
            _knockbackForce = knockbackForce;
            _lifetime = lifetime;
            _targetLayers = targetLayers;
            _initialized = true;

            // Rotate to face direction
            if (_rotateToDirection)
            {
                float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }

            // Destroy after lifetime
            Destroy(gameObject, _lifetime);
        }

        private void FixedUpdate()
        {
            if (!_initialized) return;

            _rb.MovePosition(_rb.position + _direction * _speed * Time.fixedDeltaTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Check if hit obstacle (ground layer)
            if (((1 << other.gameObject.layer) & _obstacleLayer) != 0)
            {
                OnHitObstacle();
                return;
            }

            // Check if hit target (player)
            if (((1 << other.gameObject.layer) & _targetLayers) != 0)
            {
                IDamageable damageable = other.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(_damage, _direction, _knockbackForce);
                    Debug.Log($"[BossProjectile] Hit {other.gameObject.name} for {_damage} damage!");
                }

                if (_audioManager != null)
                {
                    _audioManager.PlaySFX(_audioManager.hit);
                }

                OnHitTarget();
            }
        }

        private void OnHitTarget()
        {
            SpawnHitEffect();
            Destroy(gameObject);
        }

        private void OnHitObstacle()
        {
            SpawnHitEffect();
            Destroy(gameObject);
        }

        private void SpawnHitEffect()
        {
            if (_hitParticles != null)
            {
                ParticleSystem particles = Instantiate(_hitParticles, transform.position, Quaternion.identity);
                particles.Play();
                Destroy(particles.gameObject, particles.main.duration);
            }
        }
    }
}
