using UnityEngine;

namespace Masks
{
    /// <summary>
    /// Interface for mask abilities. Each mask can have a unique ability
    /// that is triggered when the player activates it.
    /// </summary>
    public interface IMaskAbility
    {
        /// <summary>
        /// Called when the mask is equipped by the player.
        /// Use this to initialize ability-specific state or effects.
        /// </summary>
        /// <param name="player">The player GameObject that equipped the mask.</param>
        void OnEquip(GameObject player);

        /// <summary>
        /// Called when the mask is unequipped from the player.
        /// Use this to clean up ability-specific state or effects.
        /// </summary>
        /// <param name="player">The player GameObject that unequipped the mask.</param>
        void OnUnequip(GameObject player);

        /// <summary>
        /// Called when the player triggers the mask's active ability.
        /// </summary>
        void OnAbilityTrigger();

        /// <summary>
        /// Gets the remaining cooldown time in seconds.
        /// </summary>
        float CooldownRemaining { get; }

        /// <summary>
        /// Gets whether the ability is currently active.
        /// </summary>
        bool IsAbilityActive { get; }

        /// <summary>
        /// Gets the total cooldown duration for UI progress bars.
        /// </summary>
        float TotalCooldown { get; }
    }
}
