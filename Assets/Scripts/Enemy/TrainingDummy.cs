using UnityEngine;

namespace Enemy
{
    /// <summary>
    /// A simple training dummy for testing combat.
    /// Does not move or attack, just takes damage.
    /// </summary>
    public class TrainingDummy : EnemyBase
    {
        [Header("Dummy Settings")]
        [SerializeField] private bool _resetHealthOnDeath = true;
        [SerializeField] private float _resetDelay = 2f;

        private bool _isResetting;

        protected override void Awake()
        {
            base.Awake();
            // Override enemy name if not set
            if (string.IsNullOrEmpty(EnemyName) || EnemyName == "Enemy")
            {
                // Set via reflection or just log
            }
        }

        protected override void OnDeath()
        {
            base.OnDeath();

            if (_resetHealthOnDeath && !_isResetting)
            {
                _isResetting = true;
                Invoke(nameof(ResetDummy), _resetDelay);
            }
        }

        private void ResetDummy()
        {
            if (_health != null)
            {
                _health.ResetHealth();
                Debug.Log($"[TrainingDummy] {gameObject.name} has reset!");
            }
            _isResetting = false;
        }

        /// <summary>
        /// Manually reset the dummy.
        /// </summary>
        public void ManualReset()
        {
            CancelInvoke(nameof(ResetDummy));
            _isResetting = false;
            ResetDummy();
        }
    }
}
