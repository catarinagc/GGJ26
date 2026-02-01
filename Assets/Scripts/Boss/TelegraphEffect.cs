using UnityEngine;

namespace Boss
{
    /// <summary>
    /// Special telegraph effect that shows warning before Precision Strike
    /// Creates an animated expanding/pulsing circle with inner rings
    /// </summary>
    public class TelegraphEffect : MonoBehaviour
    {
        [Header("Visual Settings")]
        [SerializeField] private Color _warningColor = new Color(1f, 0f, 0f, 0.6f);
        [SerializeField] private Color _dangerColor = new Color(1f, 0.5f, 0f, 0.8f);
        [SerializeField] private float _pulseSpeed = 4f;
        [SerializeField] private float _rotationSpeed = 90f;

        [Header("Animation")]
        [SerializeField] private float _expandDuration = 0.3f;
        [SerializeField] private float _warningDuration = 1.2f;

        private SpriteRenderer _mainRenderer;
        private SpriteRenderer _innerRingRenderer;
        private SpriteRenderer _outerRingRenderer;
        private float _startTime;
        private float _targetScale;
        private bool _isExpanding = true;

        private void Awake()
        {
            _mainRenderer = GetComponent<SpriteRenderer>();
            if (_mainRenderer == null)
            {
                _mainRenderer = gameObject.AddComponent<SpriteRenderer>();
            }

            _startTime = Time.time;
            _targetScale = transform.localScale.x;

            // Start small and expand
            transform.localScale = Vector3.zero;

            CreateInnerRings();
        }

        private void CreateInnerRings()
        {
            // Create inner ring
            GameObject innerRing = new GameObject("InnerRing");
            innerRing.transform.SetParent(transform);
            innerRing.transform.localPosition = Vector3.zero;
            innerRing.transform.localScale = Vector3.one * 0.6f;

            _innerRingRenderer = innerRing.AddComponent<SpriteRenderer>();
            _innerRingRenderer.sprite = _mainRenderer.sprite;
            _innerRingRenderer.color = new Color(_warningColor.r, _warningColor.g, _warningColor.b, _warningColor.a * 0.5f);
            _innerRingRenderer.sortingOrder = _mainRenderer.sortingOrder + 1;

            // Create outer ring
            GameObject outerRing = new GameObject("OuterRing");
            outerRing.transform.SetParent(transform);
            outerRing.transform.localPosition = Vector3.zero;
            outerRing.transform.localScale = Vector3.one * 1.2f;

            _outerRingRenderer = outerRing.AddComponent<SpriteRenderer>();
            _outerRingRenderer.sprite = _mainRenderer.sprite;
            _outerRingRenderer.color = new Color(_warningColor.r, _warningColor.g, _warningColor.b, _warningColor.a * 0.3f);
            _outerRingRenderer.sortingOrder = _mainRenderer.sortingOrder - 1;
        }

        private void Update()
        {
            float elapsed = Time.time - _startTime;

            // Expand animation
            if (_isExpanding && elapsed < _expandDuration)
            {
                float t = elapsed / _expandDuration;
                float easeOut = 1f - Mathf.Pow(1f - t, 3f); // Cubic ease out
                transform.localScale = Vector3.one * _targetScale * easeOut;
            }
            else if (_isExpanding)
            {
                _isExpanding = false;
                transform.localScale = Vector3.one * _targetScale;
            }

            // Pulse effect
            float pulse = (Mathf.Sin(elapsed * _pulseSpeed * Mathf.PI * 2f) + 1f) * 0.5f;

            // Color transition from warning to danger as time progresses
            float warningProgress = Mathf.Clamp01(elapsed / _warningDuration);
            Color currentColor = Color.Lerp(_warningColor, _dangerColor, warningProgress);

            // Apply pulsing alpha
            currentColor.a = Mathf.Lerp(0.4f, 0.9f, pulse);
            _mainRenderer.color = currentColor;

            // Animate inner rings
            if (_innerRingRenderer != null)
            {
                float innerPulse = (Mathf.Sin((elapsed + 0.25f) * _pulseSpeed * Mathf.PI * 2f) + 1f) * 0.5f;
                _innerRingRenderer.transform.localScale = Vector3.one * Mathf.Lerp(0.5f, 0.7f, innerPulse);
                Color innerColor = currentColor;
                innerColor.a *= 0.6f;
                _innerRingRenderer.color = innerColor;
            }

            if (_outerRingRenderer != null)
            {
                float outerPulse = (Mathf.Sin((elapsed - 0.25f) * _pulseSpeed * Mathf.PI * 2f) + 1f) * 0.5f;
                _outerRingRenderer.transform.localScale = Vector3.one * Mathf.Lerp(1.1f, 1.4f, outerPulse);
                Color outerColor = currentColor;
                outerColor.a *= 0.3f;
                _outerRingRenderer.color = outerColor;
            }

            // Rotate the whole thing
            transform.Rotate(0f, 0f, _rotationSpeed * Time.deltaTime);

            // Flash faster as danger approaches
            if (warningProgress > 0.7f)
            {
                float flashSpeed = Mathf.Lerp(1f, 4f, (warningProgress - 0.7f) / 0.3f);
                float flash = (Mathf.Sin(elapsed * flashSpeed * Mathf.PI * 8f) + 1f) * 0.5f;
                Color flashColor = _mainRenderer.color;
                flashColor.a = Mathf.Lerp(0.5f, 1f, flash);
                _mainRenderer.color = flashColor;
            }
        }

        public void SetWarningDuration(float duration)
        {
            _warningDuration = duration;
        }

        public void SetTargetScale(float scale)
        {
            _targetScale = scale;
        }
    }
}
