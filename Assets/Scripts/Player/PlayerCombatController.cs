using UnityEngine;
using UnityEngine.InputSystem;
using Combat;

namespace Player
{
    /// <summary>
    /// Handles combat input and delegates to PlayerCombat.
    /// Uses a single attack button for the "Magic Sword" system.
    /// </summary>
    public class PlayerCombatController : MonoBehaviour
    {
        [Header("Input Actions")]
        [SerializeField] private InputActionAsset _inputActionAsset;
        [SerializeField] private string _actionMapName = "Player";
        [SerializeField] private string _attackActionName = "Attack";
        [SerializeField] private string _aimActionName = "Move"; // Use Move for aim direction

        private InputAction _attackAction;
        private InputAction _aimAction;

        private ICombat _combat;

        private void Awake()
        {
            _combat = GetComponent<ICombat>();
            if (_combat == null)
            {
                Debug.LogError("ICombat component missing from PlayerCombatController!");
            }

            if (_inputActionAsset != null)
            {
                var map = _inputActionAsset.FindActionMap(_actionMapName);
                if (map != null)
                {
                    _attackAction = map.FindAction(_attackActionName);
                    _aimAction = map.FindAction(_aimActionName);
                }
                else
                {
                    Debug.LogError($"Action Map '{_actionMapName}' not found in Input Action Asset.");
                }
            }
            else
            {
                Debug.LogError("Input Action Asset not assigned to PlayerCombatController!");
            }
        }

        private void OnEnable()
        {
            if (_attackAction != null)
            {
                _attackAction.Enable();
                _attackAction.performed += OnAttackPerformed;
            }
            if (_aimAction != null)
            {
                _aimAction.Enable();
            }
        }

        private void OnDisable()
        {
            if (_attackAction != null)
            {
                _attackAction.performed -= OnAttackPerformed;
                _attackAction.Disable();
            }
            if (_aimAction != null)
            {
                _aimAction.Disable();
            }
        }

        private void Update()
        {
            if (_combat == null) return;

            // Update aim direction continuously (4-way snapping handled by PlayerCombat)
            Vector2 aimInput = _aimAction != null ? _aimAction.ReadValue<Vector2>() : Vector2.zero;
            _combat.SetAimDirection(aimInput);
        }

        /// <summary>
        /// Called when the attack button is pressed.
        /// Triggers the unified "Magic Sword" attack.
        /// </summary>
        private void OnAttackPerformed(InputAction.CallbackContext context)
        {
            _combat?.PerformAttack();
        }
    }
}
