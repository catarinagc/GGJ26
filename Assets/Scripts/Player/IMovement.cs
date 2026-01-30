using UnityEngine;

namespace Player
{
    public interface IMovement
    {
        /// <summary>
        /// Sets the desired movement direction.
        /// </summary>
        /// <param name="direction">Normalized direction vector.</param>
        void SetMoveInput(Vector2 direction);

        /// <summary>
        /// Initiates a jump.
        /// </summary>
        void Jump();

        /// <summary>
        /// Cuts the jump short (variable jump height).
        /// </summary>
        void CutJump();

        /// <summary>
        /// Initiates a dash in the specified direction.
        /// </summary>
        /// <param name="direction">Direction to dash.</param>
        void Dash(Vector2 direction);

        /// <summary>
        /// Checks if the character is currently grounded.
        /// </summary>
        bool IsGrounded { get; }

        /// <summary>
        /// Gets the current velocity of the character.
        /// </summary>
        Vector2 Velocity { get; }
    }
}
