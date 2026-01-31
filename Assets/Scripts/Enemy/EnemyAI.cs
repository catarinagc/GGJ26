using UnityEngine;
using Combat;

namespace Enemy
{
    /// <summary>
    /// Enemy AI with a simple State Machine: Patrol, Chase, Attack.
    /// Extends EnemyBase for health and damage handling.
    /// Now integrates with EnemyCombat for Magic Sword attacks.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyAI : EnemyBase
    {
        public enum AIState
        {
            Patrol,
            Chase,
            Attack
        }

        [Header("AI Settings")]
        [SerializeField] private AIState _currentState = AIState.Patrol;
        [SerializeField] private float _detectionRange = 8f;
        [SerializeField] private float _attackRange = 1.5f;
        [SerializeField] private float _loseTargetRange = 12f;
        [SerializeField] private LayerMask _playerLayer;

        [Header("Patrol Settings")]
        [SerializeField] private Transform _patrolPointA;
        [SerializeField] private Transform _patrolPointB;
        [SerializeField] private float _patrolSpeed = 3f;
        [SerializeField] private float _patrolWaitTime = 1f;
        [SerializeField] private bool _useLocalPatrolPoints = true;
        [SerializeField] private float _localPatrolDistance = 5f;

        [Header("Chase Settings")]
        [SerializeField] private float _chaseSpeed = 5f;

        [Header("Attack Settings")]
        [SerializeField] private float _attackTelegraphTime = 0.2f; // Brief pause before attack to telegraph
        [Tooltip("If true, uses EnemyCombat component for Magic Sword attacks. If false, uses legacy hitbox attack.")]
        [SerializeField] private bool _useMagicSwordCombat = true;

        [Header("Legacy Attack Settings (used if Magic Sword disabled)")]
        [SerializeField] private float _attackDamage = 10f;
        [SerializeField] private float _attackCooldown = 1f;
        [SerializeField] private float _attackKnockbackForce = 8f;
        [SerializeField] private Vector2 _attackHitboxSize = new Vector2(1.5f, 1f);
        [SerializeField] private Vector2 _attackHitboxOffset = new Vector2(0.8f, 0f);
        [SerializeField] private float _attackDuration = 0.3f;

        [Header("Contact Damage")]
        [SerializeField] private bool _dealContactDamage = false; // Disabled by default when using Magic Sword
        [SerializeField] private float _contactDamage = 5f;
        [SerializeField] private float _contactKnockbackForce = 6f;
        [SerializeField] private float _contactDamageCooldown = 0.5f;

        [Header("Debug")]
        [SerializeField] private bool _showDebugGizmos = true;

        // Components
        private Rigidbody2D _rb;
        private Transform _playerTransform;
        private EnemyCombat _enemyCombat;

        // Patrol State
        private Vector2 _patrolTargetPosition;
        private bool _movingToPointB = true;
        private float _patrolWaitTimer;
        private bool _isWaiting;

        // Attack State
        private float _attackCooldownTimer;
        private bool _isAttacking;
        private float _attackTimer;
        private float _telegraphTimer;
        private bool _isTelegraphing;

        // Contact Damage
        private float _contactDamageTimer;

        // Properties
        public AIState CurrentState => _currentState;

        protected override void Awake()
        {
            base.Awake();
            _rb = GetComponent<Rigidbody2D>();
            _enemyCombat = GetComponent<EnemyCombat>();

            // Configure Rigidbody2D for consistent physics
            _rb.gravityScale = 3f;
            _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            // Initialize local patrol points if not set
            if (_useLocalPatrolPoints && (_patrolPointA == null || _patrolPointB == null))
            {
                InitializeLocalPatrolPoints();
            }

            // Warn if Magic Sword combat is enabled but no EnemyCombat component
            if (_useMagicSwordCombat && _enemyCombat == null)
            {
                Debug.LogWarning($"[EnemyAI] {gameObject.name}: Magic Sword combat enabled but no EnemyCombat component found! Add EnemyCombat component or disable Magic Sword combat.");
            }
        }

        private void Start()
        {
            // Find player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                // Try to find by name if not tagged
                player = GameObject.Find("Player");
            }

            if (player != null)
            {
                _playerTransform = player.transform;
            }
            else
            {
                Debug.LogWarning($"[EnemyAI] {gameObject.name}: Could not find Player!");
            }

            // Initialize patrol target
            if (_patrolPointA != null)
            {
                _patrolTargetPosition = _patrolPointA.position;
            }
        }

        protected override void Update()
        {
            base.Update();

            if (!IsAlive) return;

            UpdateTimers();
            UpdateStateMachine();
        }

        private void FixedUpdate()
        {
            if (!IsAlive) return;

            ExecuteCurrentState();
        }

        #region State Machine

        private void UpdateStateMachine()
        {
            float distanceToPlayer = GetDistanceToPlayer();

            switch (_currentState)
            {
                case AIState.Patrol:
                    // Transition to Chase if player is in detection range
                    if (distanceToPlayer <= _detectionRange && _playerTransform != null)
                    {
                        TransitionToState(AIState.Chase);
                    }
                    break;

                case AIState.Chase:
                    // Transition to Attack if close enough and can attack
                    if (distanceToPlayer <= _attackRange && CanAttack())
                    {
                        TransitionToState(AIState.Attack);
                    }
                    // Transition back to Patrol if player is too far
                    else if (distanceToPlayer > _loseTargetRange || _playerTransform == null)
                    {
                        TransitionToState(AIState.Patrol);
                    }
                    break;

                case AIState.Attack:
                    // Transition back to Chase after attack completes
                    if (!_isAttacking && !_isTelegraphing)
                    {
                        TransitionToState(AIState.Chase);
                    }
                    break;
            }
        }

        private void ExecuteCurrentState()
        {
            switch (_currentState)
            {
                case AIState.Patrol:
                    ExecutePatrol();
                    break;

                case AIState.Chase:
                    ExecuteChase();
                    break;

                case AIState.Attack:
                    ExecuteAttack();
                    break;
            }
        }

        private void TransitionToState(AIState newState)
        {
            if (_currentState == newState) return;

            // Exit current state
            switch (_currentState)
            {
                case AIState.Patrol:
                    _isWaiting = false;
                    break;
                case AIState.Attack:
                    _isAttacking = false;
                    _isTelegraphing = false;
                    break;
            }

            Debug.Log($"[EnemyAI] {gameObject.name}: {_currentState} -> {newState}");
            _currentState = newState;

            // Enter new state
            switch (newState)
            {
                case AIState.Attack:
                    StartAttack();
                    break;
            }
        }

        #endregion

        #region Patrol

        private void InitializeLocalPatrolPoints()
        {
            // Create patrol points relative to starting position
            Vector2 startPos = transform.position;

            // Create empty GameObjects for patrol points
            GameObject pointA = new GameObject($"{gameObject.name}_PatrolA");
            pointA.transform.position = startPos + Vector2.left * _localPatrolDistance;
            _patrolPointA = pointA.transform;

            GameObject pointB = new GameObject($"{gameObject.name}_PatrolB");
            pointB.transform.position = startPos + Vector2.right * _localPatrolDistance;
            _patrolPointB = pointB.transform;

            _patrolTargetPosition = _patrolPointB.position;
        }

        private void ExecutePatrol()
        {
            if (_patrolPointA == null || _patrolPointB == null) return;

            if (_isWaiting)
            {
                _patrolWaitTimer -= Time.fixedDeltaTime;
                if (_patrolWaitTimer <= 0)
                {
                    _isWaiting = false;
                    // Switch target
                    _movingToPointB = !_movingToPointB;
                    _patrolTargetPosition = _movingToPointB ? _patrolPointB.position : _patrolPointA.position;
                }
                return;
            }

            // Move towards target
            Vector2 direction = ((Vector2)_patrolTargetPosition - (Vector2)transform.position).normalized;
            float distanceToTarget = Vector2.Distance(transform.position, _patrolTargetPosition);

            if (distanceToTarget > 0.2f)
            {
                MoveHorizontally(direction.x, _patrolSpeed);
                UpdateFacing(direction.x);
            }
            else
            {
                // Reached target, wait
                _isWaiting = true;
                _patrolWaitTimer = _patrolWaitTime;
                _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
            }
        }

        #endregion

        #region Chase

        private void ExecuteChase()
        {
            if (_playerTransform == null) return;

            Vector2 direction = ((Vector2)_playerTransform.position - (Vector2)transform.position).normalized;

            // Only move horizontally
            MoveHorizontally(direction.x, _chaseSpeed);
            UpdateFacing(direction.x);
        }

        #endregion

        #region Attack

        /// <summary>
        /// Checks if the enemy can attack based on cooldown and combat system.
        /// </summary>
        private bool CanAttack()
        {
            if (_useMagicSwordCombat && _enemyCombat != null)
            {
                return _enemyCombat.CanAttack();
            }
            return _attackCooldownTimer <= 0;
        }

        private void StartAttack()
        {
            // Stop movement during attack telegraph
            _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);

            // Face the player before attacking
            if (_playerTransform != null)
            {
                float directionToPlayer = _playerTransform.position.x - transform.position.x;
                UpdateFacing(directionToPlayer);
            }

            // Start telegraph phase (brief pause to signal attack)
            _isTelegraphing = true;
            _telegraphTimer = _attackTelegraphTime;

            Debug.Log($"[EnemyAI] {gameObject.name}: Telegraphing attack...");
        }

        private void ExecuteAttack()
        {
            // Keep stationary during attack
            _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);

            // Telegraph phase - brief pause before attack
            if (_isTelegraphing)
            {
                _telegraphTimer -= Time.fixedDeltaTime;
                if (_telegraphTimer <= 0)
                {
                    _isTelegraphing = false;
                    PerformActualAttack();
                }
                return;
            }

            // Attack duration phase
            if (_isAttacking)
            {
                _attackTimer -= Time.fixedDeltaTime;
                if (_attackTimer <= 0)
                {
                    _isAttacking = false;
                }
            }
        }

        /// <summary>
        /// Performs the actual attack using either Magic Sword combat or legacy hitbox.
        /// </summary>
        private void PerformActualAttack()
        {
            _isAttacking = true;

            if (_useMagicSwordCombat && _enemyCombat != null)
            {
                // Use Magic Sword combat system
                _enemyCombat.PerformEnemyAttack();
                _attackTimer = _enemyCombat.AttackDuration;
                Debug.Log($"[EnemyAI] {gameObject.name}: Magic Sword attack!");
            }
            else
            {
                // Use legacy hitbox attack
                _attackTimer = _attackDuration;
                _attackCooldownTimer = _attackCooldown;
                PerformLegacyAttackHitboxCheck();
                Debug.Log($"[EnemyAI] {gameObject.name}: Legacy attack!");
            }
        }

        /// <summary>
        /// Legacy attack hitbox check (used when Magic Sword combat is disabled).
        /// </summary>
        private void PerformLegacyAttackHitboxCheck()
        {
            Vector2 hitboxCenter = GetAttackHitboxCenter();

            // Find all colliders in hitbox
            Collider2D[] hits = Physics2D.OverlapBoxAll(hitboxCenter, _attackHitboxSize, 0f, _playerLayer);

            foreach (Collider2D hit in hits)
            {
                IDamageable damageable = hit.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    // Calculate knockback direction (away from enemy)
                    Vector2 knockbackDir = ((Vector2)hit.transform.position - (Vector2)transform.position).normalized;

                    Debug.Log($"[EnemyAI] {gameObject.name} LEGACY ATTACK HIT! Target: {hit.gameObject.name}, Damage: {_attackDamage}");

                    damageable.TakeDamage(_attackDamage, knockbackDir, _attackKnockbackForce);
                }
            }
        }

        private Vector2 GetAttackHitboxCenter()
        {
            Vector2 offset = _attackHitboxOffset;
            // Flip offset based on facing direction
            offset.x *= Mathf.Sign(transform.localScale.x);
            return (Vector2)transform.position + offset;
        }

        #endregion

        #region Contact Damage

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (!_dealContactDamage || !IsAlive) return;
            if (_contactDamageTimer > 0) return;

            // Check if it's the player
            if (((1 << collision.gameObject.layer) & _playerLayer) != 0 || collision.gameObject.CompareTag("Player"))
            {
                IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    Vector2 knockbackDir = ((Vector2)collision.transform.position - (Vector2)transform.position).normalized;

                    Debug.Log($"[EnemyAI] {gameObject.name} CONTACT DAMAGE! Target: {collision.gameObject.name}, Damage: {_contactDamage}");

                    damageable.TakeDamage(_contactDamage, knockbackDir, _contactKnockbackForce);
                    _contactDamageTimer = _contactDamageCooldown;
                }
            }
        }

        #endregion

        #region Helpers

        private void UpdateTimers()
        {
            if (_attackCooldownTimer > 0)
            {
                _attackCooldownTimer -= Time.deltaTime;
            }

            if (_contactDamageTimer > 0)
            {
                _contactDamageTimer -= Time.deltaTime;
            }
        }

        private float GetDistanceToPlayer()
        {
            if (_playerTransform == null) return float.MaxValue;
            return Vector2.Distance(transform.position, _playerTransform.position);
        }

        private void MoveHorizontally(float direction, float speed)
        {
            float targetVelocityX = Mathf.Sign(direction) * speed;
            _rb.linearVelocity = new Vector2(targetVelocityX, _rb.linearVelocity.y);
        }

        private void UpdateFacing(float direction)
        {
            if (Mathf.Abs(direction) > 0.1f)
            {
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Abs(scale.x) * Mathf.Sign(direction);
                transform.localScale = scale;
            }
        }

        protected override void OnDeath()
        {
            base.OnDeath();

            // Stop all movement
            _rb.linearVelocity = Vector2.zero;
            _rb.simulated = false;

            // Disable collider
            Collider2D col = GetComponent<Collider2D>();
            if (col != null)
            {
                col.enabled = false;
            }
        }

        #endregion

        #region Gizmos

        private void OnDrawGizmos()
        {
            if (!_showDebugGizmos) return;

            // Detection range (yellow)
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, _detectionRange);

            // Attack range (red)
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, _attackRange);

            // Lose target range (gray)
            Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.2f);
            Gizmos.DrawWireSphere(transform.position, _loseTargetRange);

            // Patrol points
            if (_patrolPointA != null && _patrolPointB != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(_patrolPointA.position, _patrolPointB.position);
                Gizmos.DrawWireSphere(_patrolPointA.position, 0.3f);
                Gizmos.DrawWireSphere(_patrolPointB.position, 0.3f);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!_showDebugGizmos) return;

            // Attack hitbox (only show if not using Magic Sword combat)
            if (!_useMagicSwordCombat)
            {
                Gizmos.color = _isAttacking ? Color.red : new Color(1f, 0.5f, 0f, 0.5f);
                Vector2 hitboxCenter = GetAttackHitboxCenter();
                Gizmos.DrawWireCube(hitboxCenter, _attackHitboxSize);
            }
        }

        #endregion
    }
}
