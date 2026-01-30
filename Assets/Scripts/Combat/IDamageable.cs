using UnityEngine;

namespace Combat
{
    /// <summary>
    /// Interface for any object that can receive damage.
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// Apply damage to this object.
        /// </summary>
        /// <param name="damage">Amount of damage to apply.</param>
        /// <param name="knockbackDirection">Direction of knockback force.</param>
        /// <param name="knockbackForce">Magnitude of knockback force.</param>
        void TakeDamage(float damage, Vector2 knockbackDirection, float knockbackForce);

        /// <summary>
        /// Current health of the object.
        /// </summary>
        float CurrentHealth { get; }

        /// <summary>
        /// Whether the object is still alive.
        /// </summary>
        bool IsAlive { get; }
    }
}
