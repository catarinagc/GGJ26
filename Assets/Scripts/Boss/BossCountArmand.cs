using System;
using System.Collections;
using UnityEngine;
using Combat;

namespace Boss
{
    /// <summary>
    /// Count Armand - A stationary background boss with multiple attack patterns.
    /// Uses the Magic Sword combat system for projectile-based attacks.
    /// </summary>
    public class BossCountArmand : MonoBehaviour
    {
        public enum BossState
        {
            Idle,
            Attacking,
            Transitioning
        }

        public enum AttackPattern
        {
            NobleSlashes,      // 3 large vertical waves moving horizontally
            FallingGrace,      // Projectiles from top falling toward player
            PrecisionStrike    // Telegraph + rapid strike
        }

        [Header("Boss Settings")]
        [SerializeField] private BossState _currentState = BossState.Idle;
        [SerializeField] private float _attackCooldown = 3f;
        [SerializeField] private float _patternTransitionTime = 1.5f;

        [Header("Pattern Weights")]
        [SerializeField] private float _nobleSlashesWeight = 1f;
        [SerializeField] private float _fallingGraceWeight = 1f;
        [SerializeField] private float _precisionStrikeWeight = 1f;

        [Header("Noble Slashes Settings")]
        [SerializeField] private GameObject _waveProjectilePrefab;
        [SerializeField] private float _waveSpeed = 8f;
        [SerializeField] private float _waveDamage = 15f;
        [SerializeField] private float _waveKnockback = 10f;
        [SerializeField] private float _waveSpawnInterval = 0.4f;
        [SerializeField] private int _waveCount = 3;
        [SerializeField] private Vector2 _waveSize = new Vector2(2f, 8f); // Larger waves

        [Header("Falling Grace Settings")]
        [SerializeField] private GameObject _fallingProjectilePrefab;
        [SerializeField] private float _fallingSpeed = 12f;
        [SerializeField] private float _fallingDamage = 10f;
        [SerializeField] private float _fallingKnockback = 5f;
        [SerializeField] private int _fallingProjectileCount = 5;
        [SerializeField] private float _fallingSpawnInterval = 0.2f;
        [SerializeField] private float _fallingSpreadRange = 8f;
        [SerializeField] private Transform _topSpawnPoint;

        [Header("Precision Strike Settings")]
        [SerializeField] private GameObject _telegraphPrefab;
        [SerializeField] private GameObject _strikePrefab;
        [SerializeField] private float _telegraphDuration = 1.2f;
        [SerializeField] private float _strikeDamage = 25f;
        [SerializeField] private float _strikeKnockback = 15f;
        [SerializeField] private float _strikeRadius = 2f;
        [SerializeField] private Color _telegraphColor = new Color(1f, 0f, 0f, 0.5f);

        [Header("References")]
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private LayerMask _playerLayer;
        [SerializeField] private Transform _platformCenter;

        [Header("Visual")]
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Color _attackingColor = new Color(1f, 0.5f, 0.5f, 1f);
        
        [Header("Debug")]
        [SerializeField] private bool _showDebugGizmos = true;

        // Events
        public event Action<AttackPattern> OnAttackStarted;
        public event Action<AttackPattern> OnAttackEnded;
        public event Action OnBossDefeated;

        // Components
        private Health _health;
        private Coroutine _attackCoroutine;
        private float _attackCooldownTimer;
        private Color _originalColor;
        private bool _isDefeated;

        // Properties
        public BossState CurrentState => _currentState;
        public bool IsDefeated => _isDefeated;

        private void Awake()
        {
            _health = GetComponent<Health>();
            
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (_spriteRenderer != null)
            {
                _originalColor = _spriteRenderer.color;
            }
        }

        private void Start()
        {
            // Find player if not assigned
            if (_playerTransform == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player == null)
                {
                    player = GameObject.Find("Player");
                }
                if (player != null)
                {
                    _playerTransform = player.transform;
                }
            }

            // Subscribe to health events
            if (_health != null)
            {
                _health.OnDeath += OnDeath;
            }

            // Start attack cycle
            _attackCooldownTimer = _attackCooldown * 0.5f; // Start with half cooldown
        }

        private void OnDestroy()
        {
            if (_health != null)
            {
                _health.OnDeath -= OnDeath;
            }
        }

        private void Update()
        {
            if (_isDefeated) return;

            UpdateStateMachine();
        }

        private void UpdateStateMachine()
        {
            switch (_currentState)
            {
                case BossState.Idle:
                    _attackCooldownTimer -= Time.deltaTime;
                    if (_attackCooldownTimer <= 0)
                    {
                        StartRandomAttack();
                    }
                    break;

                case BossState.Attacking:
                    // Attack coroutine handles this state
                    break;

                case BossState.Transitioning:
                    // Transition coroutine handles this state
                    break;
            }
        }

        private void StartRandomAttack()
        {
            AttackPattern pattern = SelectRandomPattern();
            StartAttack(pattern);
        }

        private AttackPattern SelectRandomPattern()
        {
            float totalWeight = _nobleSlashesWeight + _fallingGraceWeight + _precisionStrikeWeight;
            float random = UnityEngine.Random.Range(0f, totalWeight);

            if (random < _nobleSlashesWeight)
            {
                return AttackPattern.NobleSlashes;
            }
            else if (random < _nobleSlashesWeight + _fallingGraceWeight)
            {
                return AttackPattern.FallingGrace;
            }
            else
            {
                return AttackPattern.PrecisionStrike;
            }
        }

        public void StartAttack(AttackPattern pattern)
        {
            if (_attackCoroutine != null)
            {
                StopCoroutine(_attackCoroutine);
            }

            _currentState = BossState.Attacking;
            OnAttackStarted?.Invoke(pattern);

            switch (pattern)
            {
                case AttackPattern.NobleSlashes:
                    _attackCoroutine = StartCoroutine(NobleSlashesCoroutine());
                    break;
                case AttackPattern.FallingGrace:
                    _attackCoroutine = StartCoroutine(FallingGraceCoroutine());
                    break;
                case AttackPattern.PrecisionStrike:
                    _attackCoroutine = StartCoroutine(PrecisionStrikeCoroutine());
                    break;
            }
        }

        #region Noble Slashes Pattern

        private IEnumerator NobleSlashesCoroutine()
        {
            Debug.Log("[BossCountArmand] Starting Noble Slashes attack!");

            // Visual feedback
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = _attackingColor;
            }

            // Determine direction based on player position
            bool spawnFromLeft = _playerTransform != null && _playerTransform.position.x > transform.position.x;
            float spawnX = spawnFromLeft ? -20f : 20f;
            float direction = spawnFromLeft ? 1f : -1f;
            
            Debug.Log($"[BossCountArmand] Noble Slashes spawning from X={spawnX}, direction={direction}");

            // Spawn waves
            for (int i = 0; i < _waveCount; i++)
            {
                SpawnWaveProjectile(spawnX, direction, i);
                yield return new WaitForSeconds(_waveSpawnInterval);
            }

            // Wait for waves to clear
            yield return new WaitForSeconds(2f);

            EndAttack(AttackPattern.NobleSlashes);
        }

        private void SpawnWaveProjectile(float spawnX, float direction, int index)
        {
            if (_waveProjectilePrefab == null)
            {
                Debug.LogWarning("[BossCountArmand] Wave projectile prefab not assigned!");
                return;
            }

            // Spawn waves at different heights covering the play area
            // Platform is around Y=-4, player jumps up to around Y=2
            float[] waveHeights = { -2f, 0f, 2f }; // Low, mid, high
            float yPos = waveHeights[index % waveHeights.Length];
            
            // Spawn closer to the visible area (camera bounds)
            float adjustedSpawnX = spawnX > 0 ? 15f : -15f;
            Vector3 spawnPos = new Vector3(adjustedSpawnX, yPos, 0f);

            Debug.Log($"[BossCountArmand] Spawning wave {index} at position: {spawnPos}");
            
            GameObject wave = Instantiate(_waveProjectilePrefab, spawnPos, Quaternion.identity);
            
            // Configure the projectile
            BossProjectile projectile = wave.GetComponent<BossProjectile>();
            if (projectile != null)
            {
                projectile.Initialize(
                    new Vector2(direction, 0f),
                    _waveSpeed,
                    _waveDamage,
                    _waveKnockback,
                    8f, // Longer lifetime
                    _playerLayer
                );
            }

            // Set scale for large vertical wave (2x8 as requested)
            wave.transform.localScale = new Vector3(_waveSize.x, _waveSize.y, 1f);
        }

        #endregion

        #region Falling Grace Pattern

        private IEnumerator FallingGraceCoroutine()
        {
            Debug.Log("[BossCountArmand] Starting Falling Grace attack!");

            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = _attackingColor;
            }

            // Get player position for targeting
            Vector3 targetPos = _playerTransform != null ? _playerTransform.position : Vector3.zero;

            for (int i = 0; i < _fallingProjectileCount; i++)
            {
                // Update target position each spawn for tracking
                if (_playerTransform != null)
                {
                    targetPos = _playerTransform.position;
                }

                SpawnFallingProjectile(targetPos, i);
                yield return new WaitForSeconds(_fallingSpawnInterval);
            }

            // Wait for projectiles to clear
            yield return new WaitForSeconds(2f);

            EndAttack(AttackPattern.FallingGrace);
        }

        private void SpawnFallingProjectile(Vector3 targetPos, int index)
        {
            if (_fallingProjectilePrefab == null)
            {
                Debug.LogWarning("[BossCountArmand] Falling projectile prefab not assigned!");
                return;
            }

            // Calculate spawn position with spread - spawn from top of screen
            float spreadOffset = UnityEngine.Random.Range(-_fallingSpreadRange, _fallingSpreadRange);
            float spawnY = 8f; // Visible top of camera view
            Vector3 spawnPos = new Vector3(targetPos.x + spreadOffset, spawnY, 0f);

            Debug.Log($"[BossCountArmand] Spawning falling projectile at position: {spawnPos}");
            
            GameObject projectile = Instantiate(_fallingProjectilePrefab, spawnPos, Quaternion.identity);
            
            // Set scale for visible orb (1.5x1.5 as requested)
            projectile.transform.localScale = new Vector3(1.5f, 1.5f, 1f);

            BossProjectile bossProjectile = projectile.GetComponent<BossProjectile>();
            if (bossProjectile != null)
            {
                bossProjectile.Initialize(
                    Vector2.down,
                    _fallingSpeed,
                    _fallingDamage,
                    _fallingKnockback,
                    8f, // Longer lifetime
                    _playerLayer
                );
            }
        }

        #endregion

        #region Precision Strike Pattern

        private IEnumerator PrecisionStrikeCoroutine()
        {
            Debug.Log("[BossCountArmand] Starting Precision Strike attack!");

            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = _attackingColor;
            }

            // Get player position for telegraph
            Vector3 strikePos = _playerTransform != null ? _playerTransform.position : new Vector3(0f, -3f, 0f);
            strikePos.z = 0f;
            
            Debug.Log($"[BossCountArmand] Precision Strike telegraph at position: {strikePos}");

            // Spawn telegraph - larger and more visible
            GameObject telegraph = null;
            if (_telegraphPrefab != null)
            {
                telegraph = Instantiate(_telegraphPrefab, strikePos, Quaternion.identity);
                // Make telegraph much larger and visible (4x the strike radius)
                float telegraphScale = _strikeRadius * 4f;
                telegraph.transform.localScale = new Vector3(telegraphScale, telegraphScale, 1f);
                
                // Set telegraph effect duration
                TelegraphEffect telegraphEffect = telegraph.GetComponent<TelegraphEffect>();
                if (telegraphEffect != null)
                {
                    telegraphEffect.SetWarningDuration(_telegraphDuration);
                    telegraphEffect.SetTargetScale(telegraphScale);
                }
                
                // Set telegraph color
                SpriteRenderer telegraphRenderer = telegraph.GetComponent<SpriteRenderer>();
                if (telegraphRenderer != null)
                {
                    telegraphRenderer.color = _telegraphColor;
                    telegraphRenderer.sortingOrder = 99;
                }
            }

            // Wait for telegraph duration
            yield return new WaitForSeconds(_telegraphDuration);

            // Destroy telegraph
            if (telegraph != null)
            {
                Destroy(telegraph);
            }

            // Perform strike
            PerformPrecisionStrike(strikePos);

            // Wait for strike effect
            yield return new WaitForSeconds(0.8f);

            EndAttack(AttackPattern.PrecisionStrike);
        }

        private void PerformPrecisionStrike(Vector3 strikePos)
        {
            // Spawn strike visual - larger explosion
            if (_strikePrefab != null)
            {
                GameObject strike = Instantiate(_strikePrefab, strikePos, Quaternion.identity);
                // Make strike explosion much larger and visible
                float strikeScale = _strikeRadius * 4f;
                strike.transform.localScale = new Vector3(strikeScale, strikeScale, 1f);
                
                // StrikeExplosionEffect handles its own destruction
                if (strike.GetComponent<StrikeExplosionEffect>() == null)
                {
                    Destroy(strike, 0.8f);
                }
            }

            // Check for player hit with slightly larger radius for better gameplay feel
            Collider2D[] hits = Physics2D.OverlapCircleAll(strikePos, _strikeRadius * 1.2f, _playerLayer);
            foreach (Collider2D hit in hits)
            {
                IDamageable damageable = hit.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    Vector2 knockbackDir = ((Vector2)hit.transform.position - (Vector2)strikePos).normalized;
                    damageable.TakeDamage(_strikeDamage, knockbackDir, _strikeKnockback);
                    Debug.Log($"[BossCountArmand] Precision Strike hit {hit.gameObject.name}!");
                }
            }
        }

        #endregion

        private void EndAttack(AttackPattern pattern)
        {
            // Reset visual
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = _originalColor;
            }

            _currentState = BossState.Idle;
            _attackCooldownTimer = _attackCooldown;
            _attackCoroutine = null;

            OnAttackEnded?.Invoke(pattern);
            Debug.Log($"[BossCountArmand] {pattern} attack ended. Cooldown: {_attackCooldown}s");
        }

        private void OnDeath()
        {
            _isDefeated = true;
            _currentState = BossState.Idle;

            if (_attackCoroutine != null)
            {
                StopCoroutine(_attackCoroutine);
                _attackCoroutine = null;
            }

            OnBossDefeated?.Invoke();
            Debug.Log("[BossCountArmand] Boss defeated!");

            // Optional: Play death animation/effect
            StartCoroutine(DeathSequence());
        }

        private IEnumerator DeathSequence()
        {
            // Flash effect
            if (_spriteRenderer != null)
            {
                for (int i = 0; i < 5; i++)
                {
                    _spriteRenderer.color = Color.white;
                    yield return new WaitForSeconds(0.1f);
                    _spriteRenderer.color = _originalColor;
                    yield return new WaitForSeconds(0.1f);
                }

                // Fade out
                float fadeTime = 1f;
                float elapsed = 0f;
                Color startColor = _spriteRenderer.color;
                while (elapsed < fadeTime)
                {
                    elapsed += Time.deltaTime;
                    float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeTime);
                    _spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                    yield return null;
                }
            }

            // Disable the boss
            gameObject.SetActive(false);
        }

        private void OnDrawGizmos()
        {
            if (!_showDebugGizmos) return;

            // Draw platform center
            if (_platformCenter != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(_platformCenter.position, 0.5f);
            }

            // Draw top spawn point
            if (_topSpawnPoint != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(_topSpawnPoint.position, 0.5f);
                Gizmos.DrawLine(_topSpawnPoint.position + Vector3.left * _fallingSpreadRange, 
                               _topSpawnPoint.position + Vector3.right * _fallingSpreadRange);
            }

            // Draw strike radius preview
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position + Vector3.down * 5f, _strikeRadius);
        }
    }
}
