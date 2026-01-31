using UnityEngine;

namespace Masks
{
    /// <summary>
    /// ScriptableObject data container for mask definitions.
    /// Stores mask metadata, passive modifiers, and ability reference.
    /// </summary>
    [CreateAssetMenu(fileName = "NewMask", menuName = "Masked Duel/Mask Data", order = 0)]
    public class MaskData : ScriptableObject
    {
        [Header("Mask Info")]
        [SerializeField] private string _maskName = "New Mask";
        [SerializeField] private Sprite _icon;
        [SerializeField, TextArea(2, 4)] private string _description;

        [Header("Passive Modifiers")]
        [SerializeField, Tooltip("Multiplier applied to player movement speed. 1.0 = no change.")]
        private float _speedMultiplier = 1f;

        [SerializeField, Tooltip("Multiplier applied to player damage output. 1.0 = no change.")]
        private float _damageMultiplier = 1f;

        [SerializeField, Tooltip("Bonus added to player jump force.")]
        private float _jumpForceBonus = 0f;

        [Header("Ability")]
        [SerializeField, Tooltip("MonoBehaviour implementing IMaskAbility for this mask's active ability.")]
        private MonoBehaviour _abilityPrefab;

        // Public accessors
        public string MaskName => _maskName;
        public Sprite Icon => _icon;
        public string Description => _description;
        public float SpeedMultiplier => _speedMultiplier;
        public float DamageMultiplier => _damageMultiplier;
        public float JumpForceBonus => _jumpForceBonus;

        /// <summary>
        /// Gets the ability component from the ability prefab.
        /// Returns null if no ability is assigned or if it doesn't implement IMaskAbility.
        /// </summary>
        public IMaskAbility GetAbility()
        {
            if (_abilityPrefab == null) return null;
            return _abilityPrefab as IMaskAbility;
        }

        /// <summary>
        /// Gets the ability prefab for instantiation.
        /// </summary>
        public MonoBehaviour AbilityPrefab => _abilityPrefab;
    }
}
