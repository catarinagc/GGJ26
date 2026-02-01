using System;
using UnityEngine;
using Combat;

namespace Enemy
{
    /// <summary>
    /// Handles enemy combat: "Magic Sword" system with 4-way directional attacks.
    /// Mirrors the player's combat system - melee swing first, sword wave if melee misses.
    /// Supports both melee-first attacks and forced wave attacks for ranged combat.
    /// </summary>
    public class EnemyCombat : MonoBehaviour
    {
        AudioManager _audioManager;
        [Header("Combat Settings")]
        [SerializeField] private float _attackDamage = 10f;
        [SerializeField] private float _attackCooldown = 1.5f;
        [SerializeField] private float _attackDuration = 0.3f;
        [SerializeField] private float _meleeKnockbackForce = 6f;

        [Header("Projectile (Sword Wave)")]
        [SerializeField] private GameObject _projectilePrefab;
        [SerializeField] private Transform _projectileSpawnPoint;
        [SerializeField] private float _projectileDamage = 8f;
        [SerializeField] private float _projectileSpeed = 12f;
        [SerializeField] private float _projectileLifetime = 3f;
        [SerializeField] private float _rangedKnockbackForce = 4f;

        [Header("Slash Effect")]
        [SerializeField] private GameObject _slashEffectPrefab;
        [SerializeField] private Color _slashColor = new Color(0.8f, 0.1f, 0.1f, 1f); // Crimson red

        [Header("Sword Wave Color")]
        [SerializeField] private Color _swordWaveColor = new Color(0.8f, 0.15f, 0.15f, 0.9f); // Noble red

        [Header("Hitbox")]
        [SerializeField] private Vector2 _meleeHitboxSize = new Vector2(1.5f, 1f);
        [SerializeField] private Vector2 _meleeHitboxOffset = new Vector2(1f, 0f);
        [SerializeField] private LayerMask _playerLayer;

        [Header("Collision Settings")]
        [Tooltip("Layers that enemy projectiles should ignore (typically Enemy layer)")]
        [SerializeField] private LayerMask _projectileIgnoreLayers;

        [Header("Weapon Visual")]
        [SerializeField] private GameObject _weaponVisual;
        [SerializeField] private float _weaponSwingAngle = 45f;

        [Header("Debug")]
        [SerializeField] private bool _showHitboxGizmos = true;

        // Events
        public event Action OnAttackStarted;
        public event Action OnAttackEnded;
        public event Action<Vector2> OnSwordWaveFired;

        // Attack State
        private bool _isAttacking;
        private float _attackTimer;
        private float _attackCooldownTimer;

        // Aim State (4-way)
        private Vector2 _aimDirection = Vector2.right;
        private Vector2 _lastFacingDirection = Vector2.right;

        // Cached Components
        private Transform _playerTransform;
        private Collider2D _ownCollider;

        private void Awake()
        {
            _audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();
            if (_audioManager == null)
            {
                Debug.LogError("EnemyCombat: AudioManager found in scene!");
            }
            _ownCollider = GetComponent<Collider2D>();

            // Auto-create projectile spawn point if not assigned
            if (_projectileSpawnPoint == null)
            {
                GameObject spawnPoint = new GameObject("ProjectileSpawnPoint");
                spawnPoint.transform.SetParent(transform);
                spawnPoint.transform.localPosition = new Vector3(0.5f, 0f, 0f);
                _projectileSpawnPoint = spawnPoint.transform;
            }

            // Auto-set projectile ignore layers to Enemy layer if not set
            if (_projectileIgnoreLayers == 0)
            {
                int enemyLayer = LayerMask.NameToLayer("Enemy");
                if (enemyLayer >= 0)
                {
                    _projectileIgnoreLayers = 1 << enemyLayer;
                }
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
        }

        private void Update()
        {
            UpdateTimers();
            UpdateFacingDirection();
            UpdateAimDirection();

            if (_isAttacking)
            {
                HandleAttackDuration();
            }
        }

        private void UpdateTimers()
        {
            if (_attackCooldownTimer > 0)
            {
                _attackCooldownTimer -= Time.deltaTime;
            }
        }

        private void UpdateFacingDirection()
        {
            _lastFacingDirection = transform.localScale.x >= 0 ? Vector2.right : Vector2.left;
        }

        private void UpdateAimDirection()
        {
            if (_playerTransform == null) return;

            // Calculate direction to player
            Vector2 directionToPlayer = ((Vector2)_playerTransform.position - (Vector2)transform.position);

            // Snap to 4 directions
            _aimDirection = SnapTo4Directions(directionToPlayer);
        }

        private void HandleAttackDuration()
        {
            _attackTimer -= Time.deltaTime;
            if (_attackTimer <= 0)
            {
                EndAttack();
            }
        }

        #region Magic Sword Attack System

        /// <summary>
        /// Performs the unified "Magic Sword" attack.
        /// 1. Executes melee hitbox check
        /// 2. If melee hits the player, apply damage and knockback (no projectile)
        /// 3. If melee misses, spawn a sword wave projectile towards the player
        /// </summary>
        public void PerformEnemyAttack()
        {
            if (_attackCooldownTimer > 0 || _isAttacking) return;

            // Start attack
            _isAttacking = true;
            _attackTimer = _attackDuration;
            _attackCooldownTimer = _attackCooldown;

            Debug.Log($"[Enemy Magic Sword] {gameObject.name} Attack! Aim: {_aimDirection}, Damage: {_attackDamage}");

            // Fire attack started event
            OnAttackStarted?.Invoke();

            _audioManager.PlaySFX(_audioManager.enemyAttack);
            // Animate weapon swing
            AnimateWeaponSwing();

            // Perform melee hitbox check and get hit count
            int hitCount = PerformMeleeHitboxCheck(_attackDamage);

            // Spawn slash VFX
            SpawnSlashEffect();

            // Conditional Sword Wave: Only spawn projectile if melee missed
            if (hitCount == 0)
            {
                SpawnSwordWave();
            }
            else
            {
                Debug.Log($"[Enemy Magic Sword] {gameObject.name} Melee hit {hitCount} target(s) - No sword wave spawned");
            }
        }

        /// <summary>
        /// Performs a forced wave attack - always fires a projectile regardless of melee hit.
        /// Used when the enemy is at medium range and wants to fire a ranged attack.
        /// </summary>
        public void PerformWaveAttack()
        {
            if (_attackCooldownTimer > 0 || _isAttacking) return;

            // Start attack
            _isAttacking = true;
            _attackTimer = _attackDuration;
            _attackCooldownTimer = _attackCooldown;

            Debug.Log($"[Enemy Magic Sword] {gameObject.name} WAVE Attack! Aim: {_aimDirection}, Damage: {_projectileDamage}");

            // Fire attack started event
            OnAttackStarted?.Invoke();

            // Animate weapon swing
            AnimateWeaponSwing();

            // Spawn slash VFX
            SpawnSlashEffect();

            // Always spawn sword wave for wave attacks
            SpawnSwordWave();
        }

        /// <summary>
        /// Checks if the enemy can attack (not on cooldown and not currently attacking).
        /// </summary>
        public bool CanAttack()
        {
            return _attackCooldownTimer <= 0 && !_isAttacking;
        }

        /// <summary>
        /// Performs melee hitbox check and returns the number of targets hit.
        /// </summary>
        private int PerformMeleeHitboxCheck(float damage)
        {
            Vector2 hitboxCenter = GetHitboxCenter();
            Vector2 hitboxSize = GetHitboxSizeForDirection();

            // Find all colliders in hitbox
            Collider2D[] hits = Physics2D.OverlapBoxAll(hitboxCenter, hitboxSize, 0f, _playerLayer);

            int hitCount = 0;

            foreach (Collider2D hit in hits)
            {
                IDamageable damageable = hit.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    // Calculate knockback direction (away from enemy)
                    Vector2 knockbackDir = ((Vector2)hit.transform.position - (Vector2)transform.position).normalized;

                    Debug.Log($"[Enemy Magic Sword] {gameObject.name} MELEE HIT! Target: {hit.gameObject.name}, Damage: {damage}");

                    // Apply damage
                    damageable.TakeDamage(damage, knockbackDir, _meleeKnockbackForce);

                    hitCount++;
                }
            }

            return hitCount;
        }

        /// <summary>
        /// Spawns a sword wave projectile in the current 4-way aim direction.
        /// </summary>
        private void SpawnSwordWave()
        {
            if (_projectilePrefab == null)
            {
                Debug.LogWarning($"[Enemy Magic Sword] {gameObject.name}: Projectile prefab not assigned - cannot spawn sword wave.");
                return;
            }

            // Use 4-way snapped aim direction
            Vector2 fireDirection = _aimDirection;

            // Spawn projectile
            Vector3 spawnPos = _projectileSpawnPoint != null ? _projectileSpawnPoint.position : transform.position;
            GameObject projectileObj = Instantiate(_projectilePrefab, spawnPos, Quaternion.identity);

            // Set the projectile color to enemy color (crimson/red)
            SpriteRenderer projSprite = projectileObj.GetComponent<SpriteRenderer>();
            if (projSprite != null)
            {
                projSprite.color = _swordWaveColor;
            }

            // Also update SwordWaveVisual if present
            SwordWaveVisual waveVisual = projectileObj.GetComponent<SwordWaveVisual>();
            if (waveVisual != null)
            {
                waveVisual.SetColors(_swordWaveColor, new Color(_swordWaveColor.r, _swordWaveColor.g, _swordWaveColor.b, 0f));
            }

            // Setup collision ignoring - prevent projectile from hitting the enemy that fired it
            Collider2D projectileCollider = projectileObj.GetComponent<Collider2D>();
            if (projectileCollider != null && _ownCollider != null)
            {
                Physics2D.IgnoreCollision(projectileCollider, _ownCollider);
            }

            // Also ignore all other enemies
            IgnoreEnemyCollisions(projectileCollider);

            Projectile projectile = projectileObj.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.Initialize(
                    fireDirection,
                    _projectileSpeed,
                    _projectileDamage,
                    _rangedKnockbackForce,
                    _projectileLifetime,
                    _playerLayer
                );

                Debug.Log($"[Enemy Magic Sword] {gameObject.name} Sword Wave fired! Direction: {fireDirection}, Damage: {_projectileDamage}");
            }

            // Fire event
            OnSwordWaveFired?.Invoke(fireDirection);
        }

        /// <summary>
        /// Makes the projectile ignore collisions with all enemies in the scene.
        /// </summary>
        private void IgnoreEnemyCollisions(Collider2D projectileCollider)
        {
            if (projectileCollider == null) return;

            // Find all enemies and ignore their colliders
            EnemyBase[] allEnemies = FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);
            foreach (EnemyBase enemy in allEnemies)
            {
                Collider2D enemyCollider = enemy.GetComponent<Collider2D>();
                if (enemyCollider != null)
                {
                    Physics2D.IgnoreCollision(projectileCollider, enemyCollider);
                }
            }
        }

        private void SpawnSlashEffect()
        {
            if (_slashEffectPrefab == null)
            {
                Debug.LogWarning($"[Enemy Magic Sword] {gameObject.name}: SlashEffect prefab not assigned!");
                return;
            }

            Vector2 hitboxCenter = GetHitboxCenter();

            // Calculate rotation based on 4-way aim direction
            float aimAngle = Mathf.Atan2(_aimDirection.y, _aimDirection.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0, 0, aimAngle);

            GameObject slashObj = Instantiate(_slashEffectPrefab, hitboxCenter, rotation);

            // Set enemy slash color (crimson)
            SlashEffect slashEffect = slashObj.GetComponent<SlashEffect>();
            if (slashEffect != null)
            {
                slashEffect.Initialize(0.25f, _slashColor);
            }
            else
            {
                // Fallback: set sprite renderer color directly
                SpriteRenderer sr = slashObj.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.color = _slashColor;
                }
            }

            // Flip the slash effect if aiming left
            if (_aimDirection.x < 0)
            {
                Vector3 scale = slashObj.transform.localScale;
                scale.x *= -1;
                slashObj.transform.localScale = scale;
            }

            Debug.Log($"[Enemy Magic Sword] {gameObject.name} Spawning SlashEffect at {hitboxCenter}, Aim: {_aimDirection}");
        }

        private void AnimateWeaponSwing()
        {
            if (_weaponVisual == null) return;

            // Simple weapon swing animation using rotation
            float baseAngle = Mathf.Atan2(_aimDirection.y, _aimDirection.x) * Mathf.Rad2Deg;
            _weaponVisual.transform.localRotation = Quaternion.Euler(0, 0, baseAngle + _weaponSwingAngle);
        }

        private void EndAttack()
        {
            _isAttacking = false;

            // Reset weapon rotation
            if (_weaponVisual != null)
            {
                float baseAngle = Mathf.Atan2(_aimDirection.y, _aimDirection.x) * Mathf.Rad2Deg;
                _weaponVisual.transform.localRotation = Quaternion.Euler(0, 0, baseAngle);
            }

            OnAttackEnded?.Invoke();
        }

        #endregion

        #region 4-Way Aim Direction

        /// <summary>
        /// Snaps a direction vector to one of 4 cardinal directions (Up, Down, Left, Right).
        /// </summary>
        private Vector2 SnapTo4Directions(Vector2 direction)
        {
            if (direction.sqrMagnitude < 0.1f) return _lastFacingDirection;

            // Determine if horizontal or vertical is dominant
            float absX = Mathf.Abs(direction.x);
            float absY = Mathf.Abs(direction.y);

            if (absX >= absY)
            {
                // Horizontal dominant - snap to Left or Right
                return direction.x >= 0 ? Vector2.right : Vector2.left;
            }
            else
            {
                // Vertical dominant - snap to Up or Down
                return direction.y >= 0 ? Vector2.up : Vector2.down;
            }
        }

        /// <summary>
        /// Manually set the aim direction (used by AI).
        /// </summary>
        public void SetAimDirection(Vector2 direction)
        {
            if (direction.sqrMagnitude > 0.1f)
            {
                _aimDirection = SnapTo4Directions(direction);
            }
        }

        #endregion

        #region Hitbox Helpers

        /// <summary>
        /// Gets the center position of the melee hitbox in world space.
        /// Considers 4-way aim direction for hitbox positioning.
        /// </summary>
        public Vector2 GetHitboxCenter()
        {
            Vector2 offset = _meleeHitboxOffset;

            // Position hitbox based on aim direction
            if (_aimDirection == Vector2.up)
            {
                offset = new Vector2(0, Mathf.Abs(offset.x));
            }
            else if (_aimDirection == Vector2.down)
            {
                offset = new Vector2(0, -Mathf.Abs(offset.x));
            }
            else
            {
                // Horizontal - flip based on direction
                offset.x *= Mathf.Sign(_aimDirection.x);
            }

            return (Vector2)transform.position + offset;
        }

        /// <summary>
        /// Gets the hitbox size, rotated for vertical attacks.
        /// </summary>
        public Vector2 GetHitboxSizeForDirection()
        {
            // Swap width and height for vertical attacks
            if (_aimDirection == Vector2.up || _aimDirection == Vector2.down)
            {
                return new Vector2(_meleeHitboxSize.y, _meleeHitboxSize.x);
            }

            return _meleeHitboxSize;
        }

        /// <summary>
        /// Returns true if currently in an attack animation.
        /// </summary>
        public bool IsAttacking => _isAttacking;

        /// <summary>
        /// Returns the current 4-way aim direction.
        /// </summary>
        public Vector2 AimDirection => _aimDirection;

        /// <summary>
        /// Returns the attack duration for AI to know how long to pause.
        /// </summary>
        public float AttackDuration => _attackDuration;

        #endregion

        #region Gizmos

        private void OnDrawGizmos()
        {
            if (!_showHitboxGizmos) return;

            // Draw melee hitbox - RED when attacking, ORANGE when idle
            Gizmos.color = _isAttacking ? Color.red : new Color(1f, 0.5f, 0f, 0.3f);
            Vector2 center = GetHitboxCenter();
            Vector2 size = GetHitboxSizeForDirection();

            if (_isAttacking)
            {
                Gizmos.DrawCube(center, size);
            }
            else
            {
                Gizmos.DrawWireCube(center, size);
            }

            // Draw aim direction
            Gizmos.color = new Color(0.8f, 0.1f, 0.1f, 1f); // Crimson
            Vector3 aimStart = _projectileSpawnPoint != null ? _projectileSpawnPoint.position : transform.position;
            Gizmos.DrawRay(aimStart, _aimDirection * 2f);

            // Draw direction indicator
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(aimStart + (Vector3)_aimDirection * 2f, 0.15f);
        }

        private void OnDrawGizmosSelected()
        {
            if (!_showHitboxGizmos) return;

            // Draw more detailed hitbox when selected
            Gizmos.color = _isAttacking ? Color.red : new Color(1f, 0.3f, 0f, 0.8f);
            Vector2 center = GetHitboxCenter();
            Vector2 size = GetHitboxSizeForDirection();
            Gizmos.DrawWireCube(center, size);

            // Draw hitbox offset line
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, center);

            // Draw all 4 possible aim directions
            Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.3f);
            Vector3 pos = transform.position;
            Gizmos.DrawRay(pos, Vector2.up * 1.5f);
            Gizmos.DrawRay(pos, Vector2.down * 1.5f);
            Gizmos.DrawRay(pos, Vector2.left * 1.5f);
            Gizmos.DrawRay(pos, Vector2.right * 1.5f);
        }

        #endregion
    }
}
