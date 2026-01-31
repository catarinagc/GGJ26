using UnityEngine;

namespace Masks.Abilities
{
    /// <summary>
    /// Trickster Mask ability implementation.
    /// The Trickster mask grants increased speed and could have a special ability
    /// like a quick dash or afterimage effect.
    /// </summary>
    public class TricksterAbility : MonoBehaviour, IMaskAbility
    {
        [Header("Ability Settings")]
        [SerializeField] private float _abilityCooldown = 5f;
        [SerializeField] private float _abilityDuration = 2f;
        [SerializeField] private float _bonusSpeedDuringAbility = 1.5f;

        private GameObject _player;
        private float _cooldownTimer;
        private float _abilityTimer;
        private bool _isAbilityActive;

        public bool IsAbilityActive => _isAbilityActive;
        public float CooldownRemaining => _cooldownTimer;

        public void OnEquip(GameObject player)
        {
            _player = player;
            _cooldownTimer = 0f;
            _abilityTimer = 0f;
            _isAbilityActive = false;

            Debug.Log("[TricksterAbility] Equipped! Passive: +20% movement speed.");
        }

        public void OnUnequip(GameObject player)
        {
            // Clean up any active effects
            if (_isAbilityActive)
            {
                DeactivateAbility();
            }

            _player = null;
            Debug.Log("[TricksterAbility] Unequipped.");
        }

        public void OnAbilityTrigger()
        {
            if (_cooldownTimer > 0)
            {
                Debug.Log($"[TricksterAbility] Ability on cooldown: {_cooldownTimer:F1}s remaining.");
                return;
            }

            if (_isAbilityActive)
            {
                Debug.Log("[TricksterAbility] Ability already active.");
                return;
            }

            ActivateAbility();
        }

        private void Update()
        {
            // Update cooldown
            if (_cooldownTimer > 0)
            {
                _cooldownTimer -= Time.deltaTime;
            }

            // Update ability duration
            if (_isAbilityActive)
            {
                _abilityTimer -= Time.deltaTime;
                if (_abilityTimer <= 0)
                {
                    DeactivateAbility();
                }
            }
        }

        private void ActivateAbility()
        {
            _isAbilityActive = true;
            _abilityTimer = _abilityDuration;

            // TODO: Apply bonus speed effect through MaskManager or events
            // For now, just log the activation
            Debug.Log($"[TricksterAbility] ACTIVATED! Bonus speed for {_abilityDuration}s.");

            // Visual feedback could be added here (particles, color change, etc.)
        }

        private void DeactivateAbility()
        {
            _isAbilityActive = false;
            _cooldownTimer = _abilityCooldown;

            Debug.Log($"[TricksterAbility] Deactivated. Cooldown: {_abilityCooldown}s.");
        }
    }
}
