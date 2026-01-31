using UnityEngine;

namespace Combat
{
    /// <summary>
    /// Visual component for the sword wave projectile.
    /// Creates a crescent-shaped wave effect with fade and scale animation.
    /// </summary>
    public class SwordWaveVisual : MonoBehaviour
    {
        [Header("Wave Appearance")]
        [SerializeField] private Color _waveColor = new Color(0.5f, 0.8f, 1f, 0.9f);
        [SerializeField] private Color _fadeColor = new Color(0.5f, 0.8f, 1f, 0f);
        [SerializeField] private float _pulseSpeed = 8f;
        [SerializeField] private float _pulseIntensity = 0.15f;

        [Header("Trail")]
        [SerializeField] private bool _enableTrail = true;
        [SerializeField] private float _trailTime = 0.15f;
        [SerializeField] private float _trailStartWidth = 0.4f;
        [SerializeField] private float _trailEndWidth = 0f;

        private SpriteRenderer _spriteRenderer;
        private TrailRenderer _trailRenderer;
        private float _baseAlpha;
        private Vector3 _baseScale;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            SetupVisuals();
            SetupTrail();
        }

        private void SetupVisuals()
        {
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = _waveColor;
                _baseAlpha = _waveColor.a;
            }
            _baseScale = transform.localScale;
        }

        private void SetupTrail()
        {
            if (!_enableTrail) return;

            _trailRenderer = GetComponent<TrailRenderer>();
            if (_trailRenderer == null)
            {
                _trailRenderer = gameObject.AddComponent<TrailRenderer>();
            }

            // Configure trail
            _trailRenderer.time = _trailTime;
            _trailRenderer.startWidth = _trailStartWidth;
            _trailRenderer.endWidth = _trailEndWidth;
            _trailRenderer.material = new Material(Shader.Find("Sprites/Default"));
            _trailRenderer.startColor = _waveColor;
            _trailRenderer.endColor = _fadeColor;
            _trailRenderer.sortingOrder = _spriteRenderer != null ? _spriteRenderer.sortingOrder - 1 : 0;
            _trailRenderer.autodestruct = false;
            _trailRenderer.emitting = true;
        }

        private void Update()
        {
            // Pulse animation for energy effect
            float pulse = 1f + Mathf.Sin(Time.time * _pulseSpeed) * _pulseIntensity;
            transform.localScale = _baseScale * pulse;

            // Subtle alpha pulse
            if (_spriteRenderer != null)
            {
                float alphaPulse = _baseAlpha + Mathf.Sin(Time.time * _pulseSpeed * 1.5f) * 0.1f;
                Color c = _spriteRenderer.color;
                c.a = Mathf.Clamp01(alphaPulse);
                _spriteRenderer.color = c;
            }
        }

        /// <summary>
        /// Initialize the wave visual with custom color.
        /// </summary>
        public void Initialize(Color color)
        {
            _waveColor = color;
            _fadeColor = new Color(color.r, color.g, color.b, 0f);
            _baseAlpha = color.a;

            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = _waveColor;
            }

            if (_trailRenderer != null)
            {
                _trailRenderer.startColor = _waveColor;
                _trailRenderer.endColor = _fadeColor;
            }
        }

        /// <summary>
        /// Set the wave and fade colors directly.
        /// </summary>
        public void SetColors(Color waveColor, Color fadeColor)
        {
            _waveColor = waveColor;
            _fadeColor = fadeColor;
            _baseAlpha = waveColor.a;

            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = _waveColor;
            }

            if (_trailRenderer != null)
            {
                _trailRenderer.startColor = _waveColor;
                _trailRenderer.endColor = _fadeColor;
            }
        }
    }
}
