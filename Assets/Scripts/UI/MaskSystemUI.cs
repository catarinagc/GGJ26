using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Masks;

namespace UI
{
    /// <summary>
    /// UI controller for displaying mask system information.
    /// Updates in real-time to show current mask stats and ability cooldown.
    /// </summary>
    public class MaskSystemUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MaskManager _maskManager;

        [Header("UI Elements")]
        [SerializeField] private Image _maskIcon;
        [SerializeField] private Text _maskNameText;
        [SerializeField] private Text _speedText;
        [SerializeField] private Text _damageText;
        [SerializeField] private Text _abilityHintText;
        [SerializeField] private Image _cooldownBarFill;

        [Header("Colors")]
        [SerializeField] private Color _readyColor = new Color(0.2f, 0.8f, 1f, 1f);
        [SerializeField] private Color _cooldownColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        [SerializeField] private Color _activeColor = new Color(1f, 0.8f, 0.2f, 1f);
        [SerializeField] private Color _buffColor = new Color(0.4f, 1f, 0.4f, 1f);
        [SerializeField] private Color _normalColor = new Color(0.7f, 0.7f, 0.7f, 1f);

        [Header("Settings")]
        [SerializeField] private bool _hideWhenNoMask = true;

        private IMaskAbility _currentAbility;
        private CanvasGroup _canvasGroup;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            
            // Auto-find MaskManager if not assigned
            if (_maskManager == null)
            {
                _maskManager = FindAnyObjectByType<MaskManager>();
            }
        }

        private void OnEnable()
        {
            if (_maskManager != null)
            {
                _maskManager.OnMaskEquipped += OnMaskEquipped;
                _maskManager.OnMaskUnequipped += OnMaskUnequipped;
                _maskManager.OnMaskAbilityTriggered += OnAbilityTriggered;
            }
        }

        private void OnDisable()
        {
            if (_maskManager != null)
            {
                _maskManager.OnMaskEquipped -= OnMaskEquipped;
                _maskManager.OnMaskUnequipped -= OnMaskUnequipped;
                _maskManager.OnMaskAbilityTriggered -= OnAbilityTriggered;
            }
        }

        private void Start()
        {
            // Initial UI update
            if (_maskManager != null && _maskManager.CurrentMask != null)
            {
                UpdateMaskDisplay(_maskManager.CurrentMask);
                FindAbilityComponent();
            }
            else
            {
                SetUIVisibility(false);
            }
        }

        private void Update()
        {
            if (_maskManager == null || _maskManager.CurrentMask == null)
            {
                return;
            }

            UpdateStatsDisplay();
            UpdateCooldownBar();
        }

        private void OnMaskEquipped(MaskData mask)
        {
            SetUIVisibility(true);
            UpdateMaskDisplay(mask);
            FindAbilityComponent();
        }

        private void OnMaskUnequipped(MaskData mask)
        {
            _currentAbility = null;
            if (_hideWhenNoMask)
            {
                SetUIVisibility(false);
            }
        }

        private void OnAbilityTriggered()
        {
            // Visual feedback when ability is triggered
            if (_cooldownBarFill != null)
            {
                _cooldownBarFill.color = _activeColor;
            }
        }

        private void UpdateMaskDisplay(MaskData mask)
        {
            if (_maskNameText != null)
            {
                _maskNameText.text = mask.MaskName;
            }

            if (_maskIcon != null && mask.Icon != null)
            {
                _maskIcon.sprite = mask.Icon;
                _maskIcon.color = Color.white;
            }
            else if (_maskIcon != null)
            {
                // Default color when no icon
                _maskIcon.color = new Color(0.8f, 0.6f, 0.2f, 1f);
            }

            UpdateStatsDisplay();
        }

        private void UpdateStatsDisplay()
        {
            if (_maskManager == null) return;

            float speedMult = _maskManager.GetEffectiveSpeedMultiplier();
            float damageMult = _maskManager.GetEffectiveDamageMultiplier();

            if (_speedText != null)
            {
                _speedText.text = $"Speed: {speedMult:F1}x";
                _speedText.color = speedMult > 1f ? _buffColor : _normalColor;
            }

            if (_damageText != null)
            {
                _damageText.text = $"Damage: {damageMult:F1}x";
                _damageText.color = damageMult > 1f ? _buffColor : _normalColor;
            }
        }

        private void UpdateCooldownBar()
        {
            if (_cooldownBarFill == null) return;

            // Try to get cooldown info from current ability
            if (_currentAbility != null)
            {
                float cooldownRemaining = _currentAbility.CooldownRemaining;
                bool isActive = _currentAbility.IsAbilityActive;
                float totalCooldown = _currentAbility.TotalCooldown;

                if (isActive)
                {
                    // Ability is active - show full bar with active color
                    _cooldownBarFill.fillAmount = 1f;
                    _cooldownBarFill.color = _activeColor;
                    
                    if (_abilityHintText != null)
                    {
                        _abilityHintText.text = "ABILITY ACTIVE!";
                        _abilityHintText.color = _activeColor;
                    }
                }
                else if (cooldownRemaining > 0)
                {
                    // On cooldown - show progress
                    float maxCooldown = totalCooldown > 0 ? totalCooldown : 5f;
                    _cooldownBarFill.fillAmount = 1f - (cooldownRemaining / maxCooldown);
                    _cooldownBarFill.color = _cooldownColor;
                    
                    if (_abilityHintText != null)
                    {
                        _abilityHintText.text = $"Cooldown: {cooldownRemaining:F1}s";
                        _abilityHintText.color = _cooldownColor;
                    }
                }
                else
                {
                    // Ready
                    _cooldownBarFill.fillAmount = 1f;
                    _cooldownBarFill.color = _readyColor;
                    
                    if (_abilityHintText != null)
                    {
                        _abilityHintText.text = "[E] Ability Ready";
                        _abilityHintText.color = _readyColor;
                    }
                }
            }
            else
            {
                // No ability - show ready state
                _cooldownBarFill.fillAmount = 1f;
                _cooldownBarFill.color = _readyColor;
                
                if (_abilityHintText != null)
                {
                    _abilityHintText.text = "[E] Ability Ready";
                    _abilityHintText.color = _readyColor;
                }
            }
        }

        private void FindAbilityComponent()
        {
            // Get the active ability directly from MaskManager
            if (_maskManager != null)
            {
                _currentAbility = _maskManager.ActiveAbility;
            }
        }

        private void SetUIVisibility(bool visible)
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = visible ? 1f : 0f;
                _canvasGroup.interactable = visible;
                _canvasGroup.blocksRaycasts = visible;
            }
            else
            {
                gameObject.SetActive(visible);
            }
        }
    }
}
