using UnityEngine;
using System.Collections;

namespace Effects
{
    /// <summary>
    /// Handles visual hit effects like white flash when taking damage.
    /// Attach to any object with a SpriteRenderer that can be hit.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class HitEffect : MonoBehaviour
    {
        [Header("Flash Settings")]
        [SerializeField] private float _flashDuration = 0.1f;
        [SerializeField] private Color _flashColor = Color.white;
        [SerializeField] private Material _flashMaterial;

        private SpriteRenderer _spriteRenderer;
        private Material _originalMaterial;
        private Color _originalColor;
        private Coroutine _flashCoroutine;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _originalMaterial = _spriteRenderer.material;
            _originalColor = _spriteRenderer.color;
        }

        /// <summary>
        /// Trigger a white flash effect.
        /// </summary>
        public void Flash()
        {
            Flash(_flashDuration, _flashColor);
        }

        /// <summary>
        /// Trigger a flash effect with custom duration and color.
        /// </summary>
        public void Flash(float duration, Color color)
        {
            if (_flashCoroutine != null)
            {
                StopCoroutine(_flashCoroutine);
                ResetVisuals();
            }
            _flashCoroutine = StartCoroutine(FlashCoroutine(duration, color));
        }

        private IEnumerator FlashCoroutine(float duration, Color color)
        {
            // Store original color
            _originalColor = _spriteRenderer.color;

            // Apply flash - set to solid white
            _spriteRenderer.color = color;

            // If we have a flash material, use it for better effect
            if (_flashMaterial != null)
            {
                _spriteRenderer.material = _flashMaterial;
            }

            yield return new WaitForSeconds(duration);

            // Reset to original
            ResetVisuals();
            _flashCoroutine = null;
        }

        private void ResetVisuals()
        {
            _spriteRenderer.color = _originalColor;
            if (_flashMaterial != null)
            {
                _spriteRenderer.material = _originalMaterial;
            }
        }

        /// <summary>
        /// Set the original color (useful if the object's color changes dynamically).
        /// </summary>
        public void SetOriginalColor(Color color)
        {
            _originalColor = color;
        }

        private void OnDisable()
        {
            if (_flashCoroutine != null)
            {
                StopCoroutine(_flashCoroutine);
                ResetVisuals();
            }
        }
    }
}
