using UnityEngine;

namespace Combat
{
    /// <summary>
    /// ScriptableObject containing combat configuration data.
    /// Allows for data-driven design and easy tweaking in the Inspector.
    /// </summary>
    [CreateAssetMenu(fileName = "CombatData", menuName = "Masked Duel/Combat Data")]
    public class CombatData : ScriptableObject
    {
        [Header("Melee Combat")]
        [Tooltip("Damage values for each hit in the combo chain.")]
        public float[] comboDamage = { 10f, 12f, 18f };

        [Tooltip("Time window to chain the next combo hit.")]
        public float comboWindowTime = 0.5f;

        [Tooltip("Cooldown after completing or breaking the combo.")]
        public float attackCooldown = 0.3f;

        [Tooltip("Duration of each attack animation/hitbox.")]
        public float attackDuration = 0.15f;

        [Tooltip("Knockback force applied to enemies on hit.")]
        public float meleeKnockbackForce = 5f;

        [Tooltip("Recoil force applied to player on hit.")]
        public float playerRecoilForce = 2f;

        [Header("Ranged Combat")]
        [Tooltip("Damage dealt by projectiles.")]
        public float projectileDamage = 8f;

        [Tooltip("Speed of projectiles.")]
        public float projectileSpeed = 15f;

        [Tooltip("Lifetime of projectiles in seconds.")]
        public float projectileLifetime = 3f;

        [Tooltip("Cooldown between ranged shots.")]
        public float rangedCooldown = 0.2f;

        [Tooltip("Knockback force applied by projectiles.")]
        public float rangedKnockbackForce = 3f;

        [Header("Hitbox Settings")]
        [Tooltip("Size of the melee hitbox.")]
        public Vector2 meleeHitboxSize = new Vector2(1.5f, 1f);

        [Tooltip("Offset of the melee hitbox from player center.")]
        public Vector2 meleeHitboxOffset = new Vector2(1f, 0f);
    }
}
