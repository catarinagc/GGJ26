using System;
using UnityEngine;

namespace Combat
{
    /// <summary>
    /// Interface for combat systems.
    /// </summary>
    public interface ICombat
    {
        /// <summary>
        /// Event fired when an attack is performed.
        /// </summary>
        event Action<int> OnAttackPerformed; // int = combo index

        /// <summary>
        /// Event fired when a projectile is fired.
        /// </summary>
        event Action<Vector2> OnProjectileFired; // Vector2 = direction

        /// <summary>
        /// Perform a melee attack.
        /// </summary>
        void MeleeAttack();

        /// <summary>
        /// Fire a ranged projectile.
        /// </summary>
        /// <param name="direction">Direction to fire.</param>
        void RangedAttack(Vector2 direction);

        /// <summary>
        /// Set the aim direction for 8-way shooting.
        /// </summary>
        /// <param name="direction">Raw aim input.</param>
        void SetAimDirection(Vector2 direction);
    }
}
