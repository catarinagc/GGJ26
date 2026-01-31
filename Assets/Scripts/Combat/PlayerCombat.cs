using System;
using UnityEngine;
using Masks;

namespace Combat
{
    /// <summary>
    /// Handles player combat: melee combo system and 8-way ranged attacks.
    /// Decoupled from PlayerMovement using events and interfaces.
    /// </summary>
    public class PlayerCombat : MonoBehaviour, ICombat
    {
        [Header("Combat Data")]
        [SerializeField] private CombatData _combatData;

        [Header("Projectile")]
        [SerializeField] private GameObject _projectilePrefab;
        [SerializeField] private Transform _projectileSpawnPoint;

        [Header("Slash Effect")]
        [SerializeField] private GameObject _slashEffectPrefab;

        [Header("Hitbox")]
        [SerializeField] private Transform _hitboxPivot;
        [SerializeField] private LayerMask _enemyLayer;

        [Header("Mask System")]
        [SerializeField] private MaskManager _maskManager;

        [Header("Debug")]
        [SerializeField] private bool _showHitboxGizmos = true;

        // Events
        public event Action<int> OnAttackPerformed;
        public event Action<Vector2> OnProjectileFired;
        public event Action OnComboReset;

        // Combo State
        private int _currentComboIndex;
        private float _comboTimer;
        private float _attackCooldownTimer;
        private bool _isAttacking;
        private float _attackTimer;

        // Ranged State
        private float _rangedCooldownTimer;
        private Vector2 _aimDirection = Vector2.right;
        private Vector2 _lastFacingDirection = Vector2.right;

        // Cached Components
        private Rigidbody2D _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();

            if (_combatData == null)
            {
                Debug.LogWarning("CombatData not assigned to PlayerCombat. Using default values.");
            }

            // Auto-find MaskManager if not assigned
            if (_maskManager == null)
            {
                _maskManager = GetComponent<MaskManager>();
                if (_maskManager == null)
                {
                    _maskManager = FindAnyObjectByType<MaskManager>();
                }
            }
        }

        private void OnEnable()
        {
            // Subscribe to our own event to spawn slash effect
            OnAttackPerformed += SpawnSlashEffect;
        }

        private void OnDisable()
        {
            OnAttackPerformed -= SpawnSlashEffect;
        }

        private void Update()
        {
            UpdateTimers();
            UpdateFacingDirection();

            if (_isAttacking)
            {
                HandleAttackDuration();
            }
        }

        private void UpdateTimers()
        {
            // Combo window timer
            if (_comboTimer > 0)
            {
                _comboTimer -= Time.deltaTime;
                if (_comboTimer <= 0)
                {
                    ResetCombo();
                }
            }

            // Attack cooldown
            if (_attackCooldownTimer > 0)
            {
                _attackCooldownTimer -= Time.deltaTime;
            }

            // Ranged cooldown
            if (_rangedCooldownTimer > 0)
            {
                _rangedCooldownTimer -= Time.deltaTime;
            }
        }

        private void UpdateFacingDirection()
        {
            // Track facing direction based on scale (set by PlayerMovement)
            _lastFacingDirection = transform.localScale.x >= 0 ? Vector2.right : Vector2.left;
        }

        private void HandleAttackDuration()
        {
            _attackTimer -= Time.deltaTime;
            if (_attackTimer <= 0)
            {
                EndAttack();
            }
        }

        #region Melee Combat

        public void MeleeAttack()
        {
            if (_attackCooldownTimer > 0 || _isAttacking) return;

            // Start attack
            _isAttacking = true;
            _attackTimer = GetAttackDuration();

            // Get effective damage (base damage * mask multiplier)
            float effectiveDamage = GetCurrentComboDamage() * GetDamageMultiplier();

            Debug.Log($"[Combat] Melee Attack! Combo Hit: {_currentComboIndex + 1}/{GetMaxComboHits()}, Base Damage: {GetCurrentComboDamage()}, Effective Damage: {effectiveDamage}");

            // Perform hitbox check with effective damage
            PerformMeleeHitboxCheck(effectiveDamage);

            // Fire event
            OnAttackPerformed?.Invoke(_currentComboIndex);

            // Advance combo
            _currentComboIndex++;
            if (_currentComboIndex >= GetMaxComboHits())
            {
                // Combo finished, apply cooldown and reset
                _attackCooldownTimer = GetAttackCooldown();
                _currentComboIndex = 0;
                _comboTimer = 0;
            }
            else
            {
                // Start combo window for next hit
                _comboTimer = GetComboWindowTime();
            }
        }

        private void PerformMeleeHitboxCheck(float damage)
        {
            Vector2 hitboxCenter = GetHitboxCenter();
            Vector2 hitboxSize = GetHitboxSize();

            // Find all colliders in hitbox
            Collider2D[] hits = Physics2D.OverlapBoxAll(hitboxCenter, hitboxSize, 0f, _enemyLayer);

            foreach (Collider2D hit in hits)
            {
                IDamageable damageable = hit.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    // Calculate knockback direction (away from player)
                    Vector2 knockbackDir = ((Vector2)hit.transform.position - (Vector2)transform.position).normalized;
                    
                    // Debug log when hitting IDamageable
                    Debug.Log($"[Combat] HIT! Target: {hit.gameObject.name}, Damage: {damage}, Knockback: {knockbackDir}");
                    
                    // Apply damage (using effective damage with mask multiplier)
                    damageable.TakeDamage(damage, knockbackDir, GetMeleeKnockbackForce());

                    // Apply recoil to player
                    ApplyRecoil(-knockbackDir);
                }
            }
        }

        private void SpawnSlashEffect(int comboIndex)
        {
            if (_slashEffectPrefab == null) return;

            Vector2 hitboxCenter = GetHitboxCenter();
            float facingAngle = transform.localScale.x >= 0 ? 0f : 180f;
            
            // Vary the angle slightly based on combo index for visual variety
            float angleOffset = (comboIndex - 1) * 15f;
            Quaternion rotation = Quaternion.Euler(0, 0, facingAngle + angleOffset);

            GameObject slashObj = Instantiate(_slashEffectPrefab, hitboxCenter, rotation);
            
            // Flip the slash effect if facing left
            if (transform.localScale.x < 0)
            {
                Vector3 scale = slashObj.transform.localScale;
                scale.x *= -1;
                slashObj.transform.localScale = scale;
            }
        }

        private void ApplyRecoil(Vector2 direction)
        {
            if (_rb != null)
            {
                _rb.AddForce(direction * GetPlayerRecoilForce(), ForceMode2D.Impulse);
            }
        }

        private void EndAttack()
        {
            _isAttacking = false;
        }

        private void ResetCombo()
        {
            _currentComboIndex = 0;
            _comboTimer = 0;
            OnComboReset?.Invoke();
        }

        /// <summary>
        /// Gets the center position of the melee hitbox in world space.
        /// </summary>
        public Vector2 GetHitboxCenter()
        {
            Vector2 offset = GetHitboxOffset();
            // Flip offset based on facing direction
            offset.x *= Mathf.Sign(transform.localScale.x);
            return (Vector2)transform.position + offset;
        }

        /// <summary>
        /// Returns true if currently in an attack animation.
        /// </summary>
        public bool IsAttacking => _isAttacking;

        #endregion

        #region Ranged Combat

        public void SetAimDirection(Vector2 direction)
        {
            if (direction.sqrMagnitude > 0.1f)
            {
                // Snap to 8 directions (Cuphead style)
                _aimDirection = SnapTo8Directions(direction);
            }
            else
            {
                // Default to facing direction
                _aimDirection = _lastFacingDirection;
            }
        }

        public void RangedAttack(Vector2 direction)
        {
            if (_rangedCooldownTimer > 0) return;
            if (_projectilePrefab == null)
            {
                Debug.LogWarning("Projectile prefab not assigned to PlayerCombat.");
                return;
            }

            // Use aim direction if no direction provided
            Vector2 fireDirection = direction.sqrMagnitude > 0.1f ? SnapTo8Directions(direction) : _aimDirection;

            // Spawn projectile
            Vector3 spawnPos = _projectileSpawnPoint != null ? _projectileSpawnPoint.position : transform.position;
            GameObject projectileObj = Instantiate(_projectilePrefab, spawnPos, Quaternion.identity);

            Projectile projectile = projectileObj.GetComponent<Projectile>();
            if (projectile != null)
            {
                // Apply damage multiplier to projectile damage
                float effectiveProjectileDamage = GetProjectileDamage() * GetDamageMultiplier();
                
                projectile.Initialize(
                    fireDirection,
                    GetProjectileSpeed(),
                    effectiveProjectileDamage,
                    GetRangedKnockbackForce(),
                    GetProjectileLifetime(),
                    _enemyLayer
                );
            }

            // Start cooldown
            _rangedCooldownTimer = GetRangedCooldown();

            // Fire event
            OnProjectileFired?.Invoke(fireDirection);
        }

        /// <summary>
        /// Snaps a direction vector to one of 8 cardinal/diagonal directions.
        /// </summary>
        private Vector2 SnapTo8Directions(Vector2 direction)
        {
            if (direction.sqrMagnitude < 0.1f) return _lastFacingDirection;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            
            // Snap to nearest 45 degree increment
            float snappedAngle = Mathf.Round(angle / 45f) * 45f;
            float radians = snappedAngle * Mathf.Deg2Rad;

            return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)).normalized;
        }

        #endregion

        #region Mask System Integration

        /// <summary>
        /// Gets the damage multiplier from the MaskManager, or 1.0 if no mask is equipped.
        /// </summary>
        private float GetDamageMultiplier()
        {
            if (_maskManager != null)
            {
                return _maskManager.GetEffectiveDamageMultiplier();
            }
            return 1f;
        }

        /// <summary>
        /// Sets the MaskManager reference (for dependency injection or runtime assignment).
        /// </summary>
        public void SetMaskManager(MaskManager maskManager)
        {
            _maskManager = maskManager;
        }

        #endregion

        #region Data Accessors (with fallbacks)

        private float GetCurrentComboDamage()
        {
            if (_combatData == null || _combatData.comboDamage == null || _combatData.comboDamage.Length == 0)
                return 10f;
            return _combatData.comboDamage[Mathf.Clamp(_currentComboIndex, 0, _combatData.comboDamage.Length - 1)];
        }

        private int GetMaxComboHits()
        {
            if (_combatData == null || _combatData.comboDamage == null)
                return 3;
            return _combatData.comboDamage.Length;
        }

        private float GetComboWindowTime() => _combatData != null ? _combatData.comboWindowTime : 0.5f;
        private float GetAttackCooldown() => _combatData != null ? _combatData.attackCooldown : 0.3f;
        private float GetAttackDuration() => _combatData != null ? _combatData.attackDuration : 0.15f;
        private float GetMeleeKnockbackForce() => _combatData != null ? _combatData.meleeKnockbackForce : 5f;
        private float GetPlayerRecoilForce() => _combatData != null ? _combatData.playerRecoilForce : 2f;
        private float GetProjectileDamage() => _combatData != null ? _combatData.projectileDamage : 8f;
        private float GetProjectileSpeed() => _combatData != null ? _combatData.projectileSpeed : 15f;
        private float GetProjectileLifetime() => _combatData != null ? _combatData.projectileLifetime : 3f;
        private float GetRangedCooldown() => _combatData != null ? _combatData.rangedCooldown : 0.2f;
        private float GetRangedKnockbackForce() => _combatData != null ? _combatData.rangedKnockbackForce : 3f;
        private Vector2 GetHitboxSize() => _combatData != null ? _combatData.meleeHitboxSize : new Vector2(1.5f, 1f);
        private Vector2 GetHitboxOffset() => _combatData != null ? _combatData.meleeHitboxOffset : new Vector2(1f, 0f);

        #endregion

        #region Gizmos

        private void OnDrawGizmos()
        {
            if (!_showHitboxGizmos) return;

            // Draw melee hitbox - RED when attacking, YELLOW when idle
            Gizmos.color = _isAttacking ? Color.red : new Color(1f, 1f, 0f, 0.3f);
            Vector2 center = GetHitboxCenter();
            Vector2 size = GetHitboxSize();
            
            if (_isAttacking)
            {
                // Solid cube when attacking for better visibility
                Gizmos.DrawCube(center, size);
            }
            else
            {
                // Wire cube when not attacking
                Gizmos.DrawWireCube(center, size);
            }

            // Draw aim direction
            Gizmos.color = Color.cyan;
            Vector3 aimStart = _projectileSpawnPoint != null ? _projectileSpawnPoint.position : transform.position;
            Gizmos.DrawRay(aimStart, _aimDirection * 2f);
        }

        private void OnDrawGizmosSelected()
        {
            if (!_showHitboxGizmos) return;

            // Draw more detailed hitbox when selected
            Gizmos.color = _isAttacking ? Color.red : Color.yellow;
            Vector2 center = GetHitboxCenter();
            Vector2 size = GetHitboxSize();
            Gizmos.DrawWireCube(center, size);

            // Draw hitbox offset line
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, center);
        }

        #endregion
    }
}
