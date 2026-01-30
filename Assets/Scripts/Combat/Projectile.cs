using UnityEngine;

namespace Combat
{
    /// <summary>
    /// Simple projectile that moves in a direction and damages IDamageable objects.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class Projectile : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _speed = 15f;
        [SerializeField] private float _damage = 8f;
        [SerializeField] private float _knockbackForce = 3f;
        [SerializeField] private float _lifetime = 3f;
        [SerializeField] private LayerMask _targetLayers;
        [SerializeField] private LayerMask _obstacleLayer;

        private Rigidbody2D _rb;
        private Vector2 _direction;
        private bool _initialized;

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
            float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

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
            // Check if hit obstacle
            if (((1 << other.gameObject.layer) & _obstacleLayer) != 0)
            {
                OnHitObstacle();
                return;
            }

            // Check if hit target
            if (((1 << other.gameObject.layer) & _targetLayers) != 0)
            {
                IDamageable damageable = other.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(_damage, _direction, _knockbackForce);
                }
                OnHitTarget();
            }
        }

        private void OnHitTarget()
        {
            // TODO: Spawn hit effect
            Destroy(gameObject);
        }

        private void OnHitObstacle()
        {
            // TODO: Spawn impact effect
            Destroy(gameObject);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, _direction * 2f);
        }
    }
}
