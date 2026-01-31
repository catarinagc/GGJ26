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
        /// Event fired when a sword wave projectile is fired.
        /// </summary>
        event Action<Vector2> OnSwordWaveFired; // Vector2 = direction

        /// <summary>
        /// Perform a unified "Magic Sword" attack.
        /// Melee swing first - if it hits, no projectile.
        /// If melee misses, spawn a sword wave projectile.
        /// </summary>
        void PerformAttack();

        /// <summary>
        /// Set the aim direction for 4-way attacks.
        /// </summary>
        /// <param name="direction">Raw aim input.</param>
        void SetAimDirection(Vector2 direction);
    }
}
