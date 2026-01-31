using System;
using UnityEngine;
using Player;
using Combat;

namespace Masks
{
    /// <summary>
    /// Manages mask equipping, unequipping, and ability triggering.
    /// Uses events to notify other systems of mask changes.
    /// </summary>
    public class MaskManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerMovement _playerMovement;
        [SerializeField] private PlayerCombat _playerCombat;

        [Header("Current Mask")]
        [SerializeField] private MaskData _currentMask;

        [Header("Debug")]
        [SerializeField] private bool _debugMode = false;

        // Events for decoupled communication
        public event Action<MaskData> OnMaskEquipped;
        public event Action<MaskData> OnMaskUnequipped;
        public event Action OnMaskAbilityTriggered;

        // Runtime state
        private IMaskAbility _activeAbilityInstance;
        private GameObject _abilityInstanceObject;

        // Cached base stats (to restore when mask is removed)
        private float _baseSpeedMultiplier = 1f;
        private float _baseDamageMultiplier = 1f;
        private float _baseJumpForceBonus = 0f;

        // Current applied modifiers
        private float _currentSpeedMultiplier = 1f;
        private float _currentDamageMultiplier = 1f;
        private float _currentJumpForceBonus = 0f;

        public MaskData CurrentMask => _currentMask;
        public float CurrentSpeedMultiplier => _currentSpeedMultiplier;
        public float CurrentDamageMultiplier => _currentDamageMultiplier;
        public float CurrentJumpForceBonus => _currentJumpForceBonus;
        public IMaskAbility ActiveAbility => _activeAbilityInstance;

        private void Awake()
        {
            // Auto-find components if not assigned
            if (_playerMovement == null)
                _playerMovement = GetComponent<PlayerMovement>();
            if (_playerCombat == null)
                _playerCombat = GetComponent<PlayerCombat>();
        }

        private void Start()
        {
            // If a mask is already assigned in the inspector, equip it
            if (_currentMask != null)
            {
                MaskData maskToEquip = _currentMask;
                _currentMask = null; // Clear so EquipMask can properly set it
                EquipMask(maskToEquip);
            }
        }

        /// <summary>
        /// Equips a new mask, removing the current one if present.
        /// </summary>
        /// <param name="newMask">The mask data to equip.</param>
        public void EquipMask(MaskData newMask)
        {
            if (newMask == null)
            {
                Debug.LogWarning("[MaskManager] Attempted to equip null mask.");
                return;
            }

            // Unequip current mask first
            if (_currentMask != null)
            {
                UnequipCurrentMask();
            }

            _currentMask = newMask;

            // Apply passive modifiers
            ApplyPassiveModifiers(newMask);

            // Setup ability instance
            SetupAbilityInstance(newMask);

            // Notify ability of equip
            _activeAbilityInstance?.OnEquip(gameObject);

            // Fire event
            OnMaskEquipped?.Invoke(newMask);

            if (_debugMode)
            {
                Debug.Log($"[MaskManager] Equipped mask: {newMask.MaskName} | Speed: {newMask.SpeedMultiplier}x | Damage: {newMask.DamageMultiplier}x | Jump Bonus: +{newMask.JumpForceBonus}");
            }
        }

        /// <summary>
        /// Unequips the current mask and restores base stats.
        /// </summary>
        public void UnequipCurrentMask()
        {
            if (_currentMask == null) return;

            MaskData previousMask = _currentMask;

            // Notify ability of unequip
            _activeAbilityInstance?.OnUnequip(gameObject);

            // Cleanup ability instance
            CleanupAbilityInstance();

            // Remove passive modifiers
            RemovePassiveModifiers();

            // Fire event before clearing
            OnMaskUnequipped?.Invoke(previousMask);

            _currentMask = null;

            if (_debugMode)
            {
                Debug.Log($"[MaskManager] Unequipped mask: {previousMask.MaskName}");
            }
        }

        /// <summary>
        /// Swaps the current mask with a new one.
        /// </summary>
        /// <param name="newMask">The new mask to equip.</param>
        public void SwapMask(MaskData newMask)
        {
            EquipMask(newMask);
        }

        /// <summary>
        /// Triggers the active mask's ability.
        /// </summary>
        public void TriggerMaskAbility()
        {
            if (_currentMask == null)
            {
                if (_debugMode)
                    Debug.Log("[MaskManager] No mask equipped to trigger ability.");
                return;
            }

            if (_activeAbilityInstance == null)
            {
                if (_debugMode)
                    Debug.Log($"[MaskManager] Mask '{_currentMask.MaskName}' has no ability.");
                return;
            }

            _activeAbilityInstance.OnAbilityTrigger();
            OnMaskAbilityTriggered?.Invoke();

            if (_debugMode)
            {
                Debug.Log($"[MaskManager] Triggered ability for mask: {_currentMask.MaskName}");
            }
        }

        private void ApplyPassiveModifiers(MaskData mask)
        {
            _currentSpeedMultiplier = _baseSpeedMultiplier * mask.SpeedMultiplier;
            _currentDamageMultiplier = _baseDamageMultiplier * mask.DamageMultiplier;
            _currentJumpForceBonus = _baseJumpForceBonus + mask.JumpForceBonus;

            // Note: The actual application of these modifiers to PlayerMovement and PlayerCombat
            // should be done through their public APIs or by having them subscribe to our events.
            // This keeps the systems decoupled.
        }

        private void RemovePassiveModifiers()
        {
            _currentSpeedMultiplier = _baseSpeedMultiplier;
            _currentDamageMultiplier = _baseDamageMultiplier;
            _currentJumpForceBonus = _baseJumpForceBonus;
        }

        private void SetupAbilityInstance(MaskData mask)
        {
            if (mask.AbilityPrefab == null) return;

            // Instantiate the ability prefab as a child of this object
            _abilityInstanceObject = Instantiate(mask.AbilityPrefab.gameObject, transform);
            _abilityInstanceObject.name = $"{mask.MaskName}_Ability";

            _activeAbilityInstance = _abilityInstanceObject.GetComponent<IMaskAbility>();

            if (_activeAbilityInstance == null)
            {
                Debug.LogWarning($"[MaskManager] Ability prefab for '{mask.MaskName}' does not implement IMaskAbility!");
                Destroy(_abilityInstanceObject);
                _abilityInstanceObject = null;
            }
        }

        private void CleanupAbilityInstance()
        {
            if (_abilityInstanceObject != null)
            {
                Destroy(_abilityInstanceObject);
                _abilityInstanceObject = null;
            }
            _activeAbilityInstance = null;
        }

        /// <summary>
        /// Gets the effective speed multiplier (for PlayerMovement to query).
        /// </summary>
        public float GetEffectiveSpeedMultiplier()
        {
            return _currentSpeedMultiplier;
        }

        /// <summary>
        /// Gets the effective damage multiplier (for PlayerCombat to query).
        /// </summary>
        public float GetEffectiveDamageMultiplier()
        {
            return _currentDamageMultiplier;
        }

        /// <summary>
        /// Gets the effective jump force bonus (for PlayerMovement to query).
        /// </summary>
        public float GetEffectiveJumpForceBonus()
        {
            return _currentJumpForceBonus;
        }

        /// <summary>
        /// Checks if a mask is currently equipped.
        /// </summary>
        public bool HasMaskEquipped()
        {
            return _currentMask != null;
        }
    }
}
