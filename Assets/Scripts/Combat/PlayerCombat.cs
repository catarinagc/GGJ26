using System;
using UnityEngine;
using Masks;
using System.Collections;

namespace Combat
{
    /// <summary>
    /// Handles player combat: "Magic Sword" system with 4-way directional attacks.
    /// Melee swing first - if it hits an enemy, no projectile spawns.
    /// If melee misses, a sword wave projectile is fired in the aim direction.
    /// </summary>
    public class PlayerCombat : MonoBehaviour, ICombat
    {

        private AudioManager _audioManager;
        [Header("Combat Data")]
        [SerializeField] private CombatData _combatData;

        [Header("Projectile (Sword Wave)")]
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
        public event Action<Vector2> OnSwordWaveFired;

        // Combo State
        private int _currentComboIndex;
        private float _comboTimer;
        private float _attackCooldownTimer;
        private bool _isAttacking;
        private float _attackTimer;

        // Aim State (4-way)
        private Vector2 _aimDirection = Vector2.right;
        private Vector2 _lastFacingDirection = Vector2.right;

        // Cached Components
        private Rigidbody2D _rb;

        private float damageMult = 1.0f;

        public Animator animator;

        private void Awake()
        {
            _audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();
            if (_audioManager == null)
            {
                Debug.LogError("AudioManager not found in scene!");
            }
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
                //HandleAttackDuration();
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

        #region Magic Sword Attack System

        /// <summary>
        /// Performs the unified "Magic Sword" attack.
        /// 1. Executes melee hitbox check
        /// 2. If melee hits at least one IDamageable, apply damage and knockback (no projectile)
        /// 3. If melee misses, spawn a sword wave projectile in the 4-way aim direction
        /// </summary>
        //public void PerformAttack()
        //{
        //    if (_attackCooldownTimer > 0 || _isAttacking) return;

        //    // Start attack
        //    _isAttacking = true;
        //    animator.SetBool("isAttacking", true);
        //    //animator.SetTrigger("Player_Attack_1");
        //    //_attackTimer = GetAttackDuration();
        //    //AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        //    //_attackTimer = state.length;


        //    // Get effective damage (base damage * mask multiplier)
        //    float effectiveDamage = GetCurrentComboDamage() * damageMult;

        //    Debug.Log($"[Magic Sword] Attack! Combo Hit: {_currentComboIndex + 1}/{GetMaxComboHits()}, Damage: {effectiveDamage}, Aim: {_aimDirection}, Damage Mult: {damageMult}");

        //    // Perform melee hitbox check and get hit count
        //    int hitCount = PerformMeleeHitboxCheck(effectiveDamage);

        //    // Fire attack event (triggers slash VFX)
        //    OnAttackPerformed?.Invoke(_currentComboIndex);

        //    // Conditional Sword Wave: Only spawn projectile if melee missed
        //    if (hitCount == 0)
        //    {
        //        SpawnSwordWave();
        //    }
        //    else
        //    {
        //        Debug.Log($"[Magic Sword] Melee hit {hitCount} target(s) - No sword wave spawned");
        //    }

        //    // Advance combo
        //    _currentComboIndex++;
        //    if (_currentComboIndex >= GetMaxComboHits())
        //    {
        //        // Combo finished, apply cooldown and reset
        //        _attackCooldownTimer = GetAttackCooldown();
        //        _currentComboIndex = 0;
        //        _comboTimer = 0;
        //    }
        //    else
        //    {
        //        // Start combo window for next hit
        //        _comboTimer = GetComboWindowTime();
        //    }
        //}

        public void PerformAttack()
        {
            if (_attackCooldownTimer > 0 || _isAttacking) return;

            _isAttacking = true;
            animator.SetBool("isAttacking", true);

            _audioManager.PlaySFX(_audioManager.attack);


            // Perform melee check immediately
            int hitCount = PerformMeleeHitboxCheck(GetCurrentComboDamage() * damageMult);

            // Only spawn projectile if melee missed
            if (hitCount == 0)
            {
                StartCoroutine(FireProjectileWithDelay(0.3f)); // 0.3s delay like enemy
            }

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

        private IEnumerator FireProjectileWithDelay(float delay)
        {
            // Optional: play a telegraph effect here before firing
            yield return new WaitForSeconds(delay);

            SpawnSwordWave();
        }


        /// <summary>
        /// Performs melee hitbox check and returns the number of enemies hit.
        /// </summary>
        private int PerformMeleeHitboxCheck(float damage)
        {
            Vector2 hitboxCenter = GetHitboxCenter();
            Vector2 hitboxSize = GetHitboxSize();

            // Find all colliders in hitbox
            Collider2D[] hits = Physics2D.OverlapBoxAll(hitboxCenter, hitboxSize, 0f, _enemyLayer);

            int hitCount = 0;

            foreach (Collider2D hit in hits)
            {
                IDamageable damageable = hit.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    // Calculate knockback direction (away from player)
                    Vector2 knockbackDir = ((Vector2)hit.transform.position - (Vector2)transform.position).normalized;

                    Debug.Log($"[Magic Sword] MELEE HIT! Target: {hit.gameObject.name}, Damage: {damage}");

                    // Apply damage
                    damageable.TakeDamage(damage, knockbackDir, GetMeleeKnockbackForce());

                    // Apply recoil to player
                    ApplyRecoil(-knockbackDir);

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
                Debug.LogWarning("Projectile prefab not assigned - cannot spawn sword wave.");
                return;
            }

            // Use 4-way snapped aim direction
            Vector2 fireDirection = _aimDirection;

            // Spawn projectile
            Vector3 spawnPos = _projectileSpawnPoint != null ? _projectileSpawnPoint.position : transform.position;
            GameObject projectileObj = Instantiate(_projectilePrefab, spawnPos, Quaternion.identity);

            Projectile projectile = projectileObj.GetComponent<Projectile>();
            if (projectile != null)
            {
                // Apply damage multiplier to projectile damage
                float effectiveProjectileDamage = GetProjectileDamage() * damageMult;

                projectile.Initialize(
                    fireDirection,
                    GetProjectileSpeed(),
                    effectiveProjectileDamage,
                    GetRangedKnockbackForce(),
                    GetProjectileLifetime(),
                    _enemyLayer
                );

                Debug.Log($"[Magic Sword] Sword Wave fired! Direction: {fireDirection}, Damage: {effectiveProjectileDamage}");
            }

            // Fire event
            OnSwordWaveFired?.Invoke(fireDirection);
        }

        private void SpawnSlashEffect(int comboIndex)
        {
            if (_slashEffectPrefab == null)
            {
                Debug.LogWarning("[Magic Sword] SlashEffect prefab not assigned!");
                return;
            }

            Vector2 hitboxCenter = GetHitboxCenter();
            Debug.Log($"[Magic Sword] Spawning SlashEffect at {hitboxCenter}, Aim: {_aimDirection}");

            // Calculate rotation based on 4-way aim direction
            float aimAngle = Mathf.Atan2(_aimDirection.y, _aimDirection.x) * Mathf.Rad2Deg;

            // Vary the angle slightly based on combo index for visual variety
            float angleOffset = (comboIndex - 1) * 15f;
            Quaternion rotation = Quaternion.Euler(0, 0, aimAngle + angleOffset);

            GameObject slashObj = Instantiate(_slashEffectPrefab, hitboxCenter, rotation);

            // Flip the slash effect if aiming left
            if (_aimDirection.x < 0)
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

        public void EndAttack()
        {
            bool isAttacking1 = animator.GetBool("isAttack1");
            animator.SetBool("isAttack1", !isAttacking1);
            _isAttacking = false;
            animator.SetBool("isAttacking", false);
        }

        private void ResetCombo()
        {
            _currentComboIndex = 0;
            _comboTimer = 0;
        }

        #endregion

        #region 4-Way Aim Direction

        /// <summary>
        /// Sets the aim direction, snapping to 4 cardinal directions (Up, Down, Left, Right).
        /// </summary>
        public void SetAimDirection(Vector2 direction)
        {
            if (direction.sqrMagnitude > 0.1f)
            {
                // Snap to 4 directions
                _aimDirection = SnapTo4Directions(direction);
            }
            else
            {
                // Default to facing direction (left or right based on character facing)
                _aimDirection = _lastFacingDirection;
            }
        }

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

        #endregion

        #region Hitbox Helpers

        /// <summary>
        /// Gets the center position of the melee hitbox in world space.
        /// Now considers 4-way aim direction for hitbox positioning.
        /// </summary>
        public Vector2 GetHitboxCenter()
        {
            Vector2 offset = GetHitboxOffset();

            // Position hitbox based on aim direction
            if (_aimDirection == Vector2.up)
            {
                // Swap X and Y for vertical attacks
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
            Vector2 size = GetHitboxSize();

            // Swap width and height for vertical attacks
            if (_aimDirection == Vector2.up || _aimDirection == Vector2.down)
            {
                return new Vector2(size.y, size.x);
            }

            return size;
        }

        /// <summary>
        /// Returns true if currently in an attack animation.
        /// </summary>
        public bool IsAttacking => _isAttacking;

        /// <summary>
        /// Returns the current 4-way aim direction.
        /// </summary>
        public Vector2 AimDirection => _aimDirection;

        #endregion

        #region Mask System Integration

        /// <summary>
        /// Gets the damage multiplier from the MaskManager, or 1.0 if no mask is equipped.
        /// </summary>
        //private float GetDamageMultiplier()
        //{
        //    //if (_maskManager != null)
        //    //{
        //    //    return _maskManager.GetEffectiveDamageMultiplier();
        //    //}
        //    //return 1f;
        //    retu
        //}

        public void changeDamageMult(float extraMult)
        {
            damageMult += extraMult;
            Debug.Log(damageMult);
        }

        /// <summary>
        /// Sets the MaskManager reference (for dependency injection or runtime assignment).
        /// </summary>
        //public void SetMaskManager(MaskManager maskManager)
        //{
        //    _maskManager = maskManager;
        //}

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
            Vector2 size = GetHitboxSizeForDirection();

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

            // Draw aim direction (4-way)
            Gizmos.color = Color.cyan;
            Vector3 aimStart = _projectileSpawnPoint != null ? _projectileSpawnPoint.position : transform.position;
            Gizmos.DrawRay(aimStart, _aimDirection * 2f);

            // Draw direction indicator
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(aimStart + (Vector3)_aimDirection * 2f, 0.15f);
        }

        private void OnDrawGizmosSelected()
        {
            if (!_showHitboxGizmos) return;

            // Draw more detailed hitbox when selected
            Gizmos.color = _isAttacking ? Color.red : Color.yellow;
            Vector2 center = GetHitboxCenter();
            Vector2 size = GetHitboxSizeForDirection();
            Gizmos.DrawWireCube(center, size);

            // Draw hitbox offset line
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, center);

            // Draw all 4 possible aim directions
            Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
            Vector3 pos = transform.position;
            Gizmos.DrawRay(pos, Vector2.up * 1.5f);
            Gizmos.DrawRay(pos, Vector2.down * 1.5f);
            Gizmos.DrawRay(pos, Vector2.left * 1.5f);
            Gizmos.DrawRay(pos, Vector2.right * 1.5f);
        }

        #endregion

    }
}
