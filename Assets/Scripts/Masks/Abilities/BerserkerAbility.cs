using UnityEngine;

namespace Masks.Abilities
{
    /// <summary>
    /// Berserker Mask ability implementation.
    /// The Berserker mask grants increased damage but could have drawbacks.
    /// Active ability: Rage mode - temporary massive damage boost.
    /// </summary>
    public class BerserkerAbility : MonoBehaviour, IMaskAbility
    {
        [Header("Ability Settings")]
        [SerializeField] private float _abilityCooldown = 8f;
        [SerializeField] private float _abilityDuration = 3f;
        [SerializeField] private float _rageDamageMultiplier = 2f;

        private GameObject _player;
        private float _cooldownTimer;
        private float _abilityTimer;
        private bool _isAbilityActive;

        public bool IsAbilityActive => _isAbilityActive;
        public float CooldownRemaining => _cooldownTimer;
        public float TotalCooldown => _abilityCooldown;

        public void OnEquip(GameObject player)
        {
            _player = player;
            _cooldownTimer = 0f;
            _abilityTimer = 0f;
            _isAbilityActive = false;

            Debug.Log("[BerserkerAbility] Equipped! Passive: +50% damage output.");
        }

        public void OnUnequip(GameObject player)
        {
            if (_isAbilityActive)
            {
                DeactivateAbility();
            }

            _player = null;
            Debug.Log("[BerserkerAbility] Unequipped.");
        }

        public void OnAbilityTrigger()
        {
            if (_cooldownTimer > 0)
            {
                Debug.Log($"[BerserkerAbility] Ability on cooldown: {_cooldownTimer:F1}s remaining.");
                return;
            }

            if (_isAbilityActive)
            {
                Debug.Log("[BerserkerAbility] Ability already active.");
                return;
            }

            ActivateAbility();
        }

        private void Update()
        {
            if (_cooldownTimer > 0)
            {
                _cooldownTimer -= Time.deltaTime;
            }

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

            Debug.Log($"[BerserkerAbility] RAGE ACTIVATED! {_rageDamageMultiplier}x damage for {_abilityDuration}s!");
        }

        private void DeactivateAbility()
        {
            _isAbilityActive = false;
            _cooldownTimer = _abilityCooldown;

            Debug.Log($"[BerserkerAbility] Rage ended. Cooldown: {_abilityCooldown}s.");
        }
    }
}
