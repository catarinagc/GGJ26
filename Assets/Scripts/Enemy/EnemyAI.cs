using UnityEngine;
using Combat;

namespace Enemy
{
    /// <summary>
    /// Enemy AI with a simple State Machine: Patrol, Chase, Attack.
    /// Extends EnemyBase for health and damage handling.
    /// Now integrates with EnemyCombat for Magic Sword attacks with dual attack ranges.
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

        public enum AttackType
        {
            Melee,
            Wave
        }

        [Header("AI Settings")]
        [SerializeField] private AIState _currentState = AIState.Patrol;
        [SerializeField] private float _detectionRange = 8f;
        [SerializeField] private float _loseTargetRange = 12f;
        [SerializeField] private LayerMask _playerLayer;

        [Header("Dual Attack Ranges")]
        [Tooltip("Range for melee attacks - enemy must be this close to attempt melee")]
        [SerializeField] private float _meleeAttackRange = 1.5f;
        [Tooltip("Range for wave attacks - enemy can fire projectiles from this distance")]
        [SerializeField] private float _waveAttackRange = 6f;
        [Tooltip("Chance (0-1) to fire a wave attack when in wave range but outside melee range")]
        [SerializeField] private float _waveAttackChance = 0.7f;
        [Tooltip("Minimum time between wave attack attempts when chasing")]
        [SerializeField] private float _waveAttackInterval = 2f;

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
        [SerializeField] private float _meleeTelegraphTime = 0.2f;
        [SerializeField] private float _waveTelegraphTime = 0.35f;
        [Tooltip("If true, uses EnemyCombat component for Magic Sword attacks. If false, uses legacy hitbox attack.")]
        [SerializeField] private bool _useMagicSwordCombat = true;

        [Header("Telegraph Visual")]
        [SerializeField] private Color _telegraphColor = new Color(1f, 0.9f, 0.3f, 1f); // Yellow tint
        [SerializeField] private bool _useTelegraphVisual = true;

        [Header("Legacy Attack Settings (used if Magic Sword disabled)")]
        [SerializeField] private float _attackDamage = 10f;
        [SerializeField] private float _attackCooldown = 1f;
        [SerializeField] private float _attackKnockbackForce = 8f;
        [SerializeField] private Vector2 _attackHitboxSize = new Vector2(1.5f, 1f);
        [SerializeField] private Vector2 _attackHitboxOffset = new Vector2(0.8f, 0f);
        [SerializeField] private float _attackDuration = 0.3f;

        [Header("Contact Damage")]
        [SerializeField] private bool _dealContactDamage = false;
        [SerializeField] private float _contactDamage = 5f;
        [SerializeField] private float _contactKnockbackForce = 6f;
        [SerializeField] private float _contactDamageCooldown = 0.5f;

        [Header("Debug")]
        [SerializeField] private bool _showDebugGizmos = true;

        public Animator animator;


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
        private AttackType _currentAttackType;
        private float _waveAttackTimer;

        // Contact Damage
        private float _contactDamageTimer;

        // Visual State
        private bool _isShowingTelegraph;

        // Properties
        public AIState CurrentState => _currentState;
        public AttackType CurrentAttackType => _currentAttackType;

        protected override void Awake()
        {
            base.Awake();

            //_audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();
            //if (_audioManager == null)
            //{
            //    Debug.LogWarning($"[EnemyAI] {gameObject.name}: Could not find AudioManager!");
            //}
            _rb = GetComponent<Rigidbody2D>();
            _enemyCombat = GetComponent<EnemyCombat>();
            //_spriteRenderer = GetComponent<SpriteRenderer>();

            // Store original color for telegraph visual
            //if (_spriteRenderer != null)
            //{
            //    _originalColor = _spriteRenderer.color;
            //}

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
            //if (_patrolPointA != null)
            //{
            //    _patrolTargetPosition = _patrolPointA.position;
            //}

            if (_patrolPointA != null && _patrolPointB != null)
            {
                _patrolTargetPosition = _movingToPointB
                    ? _patrolPointB.position
                    : _patrolPointA.position;
            }


            // Initialize wave attack timer with some randomness
            _waveAttackTimer = Random.Range(0f, _waveAttackInterval * 0.5f);
        }

        protected override void Update()
        {
            float speed = Mathf.Abs(_rb.linearVelocity.x);
            if (speed < 0.05f)
                speed = 0f;

            animator.SetFloat("Speed", speed);
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
                    // Check for attack opportunities
                    if (CanAttack())
                    {
                        // Priority 1: Melee attack if very close
                        if (distanceToPlayer <= _meleeAttackRange)
                        {
                            _currentAttackType = AttackType.Melee;
                            TransitionToState(AIState.Attack);
                        }
                        // Priority 2: Wave attack if in wave range but outside melee range
                        else if (distanceToPlayer <= _waveAttackRange && distanceToPlayer > _meleeAttackRange)
                        {
                            // Check wave attack timer and chance
                            if (_waveAttackTimer <= 0 && Random.value <= _waveAttackChance)
                            {
                                _currentAttackType = AttackType.Wave;
                                TransitionToState(AIState.Attack);
                            }
                        }
                    }
                    // Transition back to Patrol if player is too far
                    if (distanceToPlayer > _loseTargetRange || _playerTransform == null)
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
                    EndTelegraphVisual();
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
            Vector2 startPos = transform.position;

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
                    _movingToPointB = !_movingToPointB;
                    _patrolTargetPosition = _movingToPointB ? _patrolPointB.position : _patrolPointA.position;
                }
                return;
            }

            Vector2 delta = (Vector2)_patrolTargetPosition - (Vector2)transform.position;

            float directionX = Mathf.Sign(delta.x);

            float distanceToTarget = Vector2.Distance(transform.position, _patrolTargetPosition);

            if (Mathf.Abs(delta.x) > 0.1f)
            {
                MoveHorizontally(directionX, _patrolSpeed);
                UpdateFacing(directionX);
                _audioManager.PlaySFX(_audioManager.enemySteps);
            }
            else
            {
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
            _audioManager.PlaySFX(_audioManager.enemySteps);
            MoveHorizontally(direction.x, _chaseSpeed);
            UpdateFacing(direction.x);
        }

        #endregion

        #region Attack

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

            animator.SetBool("isAttacking", true);

            // Stop movement during attack telegraph
            _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);

            // Face the player before attacking
            if (_playerTransform != null)
            {
                float directionToPlayer = _playerTransform.position.x - transform.position.x;
                UpdateFacing(directionToPlayer);
            }

            // Start telegraph phase with appropriate duration based on attack type
            _isTelegraphing = true;
            _telegraphTimer = (_currentAttackType == AttackType.Wave) ? _waveTelegraphTime : _meleeTelegraphTime;

            // Start telegraph visual
            StartTelegraphVisual();

            string attackTypeStr = _currentAttackType == AttackType.Wave ? "WAVE" : "MELEE";
            Debug.Log($"[EnemyAI] {gameObject.name}: Telegraphing {attackTypeStr} attack for {_telegraphTimer}s...");
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
                    EndTelegraphVisual();
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

        private void PerformActualAttack()
        {
            _isAttacking = true;

            // Reset wave attack timer after any attack
            _waveAttackTimer = _waveAttackInterval;

            if (_useMagicSwordCombat && _enemyCombat != null)
            {
                // Use Magic Sword combat system
                // For wave attacks, we force the projectile by calling a special method
                if (_currentAttackType == AttackType.Wave)
                {
                    _enemyCombat.PerformWaveAttack();
                }
                else
                {
                    _enemyCombat.PerformEnemyAttack();
                }
                _attackTimer = _enemyCombat.AttackDuration;

                string attackTypeStr = _currentAttackType == AttackType.Wave ? "WAVE" : "MELEE";
                Debug.Log($"[EnemyAI] {gameObject.name}: Magic Sword {attackTypeStr} attack!");
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

        private void PerformLegacyAttackHitboxCheck()
        {
            Vector2 hitboxCenter = GetAttackHitboxCenter();

            Collider2D[] hits = Physics2D.OverlapBoxAll(hitboxCenter, _attackHitboxSize, 0f, _playerLayer);

            foreach (Collider2D hit in hits)
            {
                IDamageable damageable = hit.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    Vector2 knockbackDir = ((Vector2)hit.transform.position - (Vector2)transform.position).normalized;

                    Debug.Log($"[EnemyAI] {gameObject.name} LEGACY ATTACK HIT! Target: {hit.gameObject.name}, Damage: {_attackDamage}");

                    damageable.TakeDamage(_attackDamage, knockbackDir, _attackKnockbackForce);
                }
            }
        }

        private Vector2 GetAttackHitboxCenter()
        {
            Vector2 offset = _attackHitboxOffset;
            offset.x *= Mathf.Sign(transform.localScale.x);
            return (Vector2)transform.position + offset;
        }

        #endregion

        #region Telegraph Visual

        private void StartTelegraphVisual()
        {
            if (!_useTelegraphVisual || _spriteRenderer == null) return;

            _isShowingTelegraph = true;
            _spriteRenderer.color = _telegraphColor;
        }

        private void EndTelegraphVisual()
        {
            if (!_isShowingTelegraph || _spriteRenderer == null) return;

            _isShowingTelegraph = false;
            _spriteRenderer.color = _originalColor;
        }

        #endregion

        #region Contact Damage

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (!_dealContactDamage || !IsAlive) return;
            if (_contactDamageTimer > 0) return;

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

            // Wave attack interval timer (only counts down while chasing)
            if (_currentState == AIState.Chase && _waveAttackTimer > 0)
            {
                _waveAttackTimer -= Time.deltaTime;
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

            // End any telegraph visual
            EndTelegraphVisual();

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

            // Wave attack range (orange)
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, _waveAttackRange);

            // Melee attack range (red)
            Gizmos.color = new Color(1f, 0f, 0f, 0.4f);
            Gizmos.DrawWireSphere(transform.position, _meleeAttackRange);

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

            // Draw labels for ranges
#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * (_meleeAttackRange + 0.3f), "Melee Range");
            UnityEditor.Handles.Label(transform.position + Vector3.up * (_waveAttackRange + 0.3f), "Wave Range");
#endif
        }

        #endregion

        public void EndAttack()
        {
            animator.SetBool("isAttacking", false);
            bool isAttack1 = animator.GetBool("isAttack1");
            animator.SetBool("isAttack1", !isAttack1);
        }
    }
}
