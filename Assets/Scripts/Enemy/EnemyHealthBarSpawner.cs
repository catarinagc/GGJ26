using UnityEngine;
using Combat;
using UI;

namespace Enemy
{
    /// <summary>
    /// Spawns a floating health bar above the enemy when they take damage.
    /// Attach to any enemy with a Health component.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class EnemyHealthBarSpawner : MonoBehaviour
    {
        [Header("Health Bar Prefab")]
        [SerializeField] private GameObject _healthBarPrefab;

        [Header("Settings")]
        [SerializeField] private Vector3 _offset = new Vector3(0f, 1.2f, 0f);
        [SerializeField] private bool _spawnOnStart = false;
        [SerializeField] private bool _spawnOnFirstDamage = true;

        private Health _health;
        private GameObject _healthBarInstance;
        private bool _hasSpawned;

        private void Awake()
        {
            _health = GetComponent<Health>();
        }

        private void OnEnable()
        {
            if (_health != null)
            {
                _health.OnDamageTaken += OnDamageTaken;
            }
        }

        private void OnDisable()
        {
            if (_health != null)
            {
                _health.OnDamageTaken -= OnDamageTaken;
            }
        }

        private void Start()
        {
            if (_spawnOnStart)
            {
                SpawnHealthBar();
            }
        }

        private void OnDamageTaken(float damage)
        {
            if (_spawnOnFirstDamage && !_hasSpawned)
            {
                SpawnHealthBar();
            }
        }

        private void SpawnHealthBar()
        {
            if (_hasSpawned || _healthBarPrefab == null) return;

            _healthBarInstance = Instantiate(_healthBarPrefab, transform.position + _offset, Quaternion.identity);
            
            EnemyHealthBar healthBar = _healthBarInstance.GetComponent<EnemyHealthBar>();
            if (healthBar != null)
            {
                healthBar.Initialize(_health, transform);
            }

            _hasSpawned = true;
        }

        private void OnDestroy()
        {
            if (_healthBarInstance != null)
            {
                Destroy(_healthBarInstance);
            }
        }

        /// <summary>
        /// Set the health bar prefab at runtime.
        /// </summary>
        public void SetHealthBarPrefab(GameObject prefab)
        {
            _healthBarPrefab = prefab;
        }
    }
}
