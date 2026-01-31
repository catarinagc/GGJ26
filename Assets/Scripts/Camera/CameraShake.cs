using UnityEngine;
using System.Collections;

namespace CameraSystem
{
    /// <summary>
    /// Camera shake effect for impact feedback.
    /// Can be triggered on hits, explosions, or other impactful events.
    /// </summary>
    public class CameraShake : MonoBehaviour
    {
        [Header("Default Settings")]
        [SerializeField] private float _defaultDuration = 0.1f;
        [SerializeField] private float _defaultMagnitude = 0.1f;

        [Header("Shake Settings")]
        [SerializeField] private float _dampingSpeed = 1.0f;
        [SerializeField] private bool _useUnscaledTime = false;

        private Vector3 _originalLocalPosition;
        private Coroutine _shakeCoroutine;
        private float _currentShakeMagnitude;
        private float _currentShakeDuration;

        // Singleton for easy access
        public static CameraShake Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                // Allow multiple instances, just don't set as singleton
            }

            _originalLocalPosition = transform.localPosition;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// Trigger a camera shake with default settings.
        /// </summary>
        public void Shake()
        {
            Shake(_defaultDuration, _defaultMagnitude);
        }

        /// <summary>
        /// Trigger a camera shake with custom duration and magnitude.
        /// </summary>
        /// <param name="duration">How long the shake lasts in seconds.</param>
        /// <param name="magnitude">How intense the shake is.</param>
        public void Shake(float duration, float magnitude)
        {
            // If already shaking, use the stronger shake
            if (_shakeCoroutine != null)
            {
                if (magnitude > _currentShakeMagnitude)
                {
                    StopCoroutine(_shakeCoroutine);
                    _shakeCoroutine = StartCoroutine(ShakeCoroutine(duration, magnitude));
                }
                // Otherwise let the current shake continue
            }
            else
            {
                _shakeCoroutine = StartCoroutine(ShakeCoroutine(duration, magnitude));
            }
        }

        private IEnumerator ShakeCoroutine(float duration, float magnitude)
        {
            _currentShakeDuration = duration;
            _currentShakeMagnitude = magnitude;

            float elapsed = 0f;

            while (elapsed < duration)
            {
                // Calculate shake offset
                float x = Random.Range(-1f, 1f) * _currentShakeMagnitude;
                float y = Random.Range(-1f, 1f) * _currentShakeMagnitude;

                // Apply shake offset to local position
                transform.localPosition = _originalLocalPosition + new Vector3(x, y, 0f);

                // Reduce magnitude over time for smooth falloff
                _currentShakeMagnitude = Mathf.Lerp(magnitude, 0f, elapsed / duration);

                if (_useUnscaledTime)
                {
                    elapsed += Time.unscaledDeltaTime;
                    yield return null;
                }
                else
                {
                    elapsed += Time.deltaTime;
                    yield return null;
                }
            }

            // Reset to original position
            transform.localPosition = _originalLocalPosition;
            _shakeCoroutine = null;
            _currentShakeMagnitude = 0f;
        }

        /// <summary>
        /// Stop any ongoing shake and reset position.
        /// </summary>
        public void StopShake()
        {
            if (_shakeCoroutine != null)
            {
                StopCoroutine(_shakeCoroutine);
                _shakeCoroutine = null;
            }
            transform.localPosition = _originalLocalPosition;
            _currentShakeMagnitude = 0f;
        }

        /// <summary>
        /// Update the original position (call this if the camera moves to a new base position).
        /// </summary>
        public void UpdateOriginalPosition()
        {
            _originalLocalPosition = transform.localPosition;
        }

        /// <summary>
        /// Static method for easy access from anywhere.
        /// </summary>
        public static void TriggerShake(float duration, float magnitude)
        {
            if (Instance != null)
            {
                Instance.Shake(duration, magnitude);
            }
        }
    }
}
