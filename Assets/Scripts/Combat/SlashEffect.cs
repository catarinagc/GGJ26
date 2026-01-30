using UnityEngine;

namespace Combat
{
    /// <summary>
    /// Temporary visual effect for sword slashes.
    /// Spawns a white semi-circle sprite and destroys itself after a short duration.
    /// </summary>
    public class SlashEffect : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _lifetime = 0.1f;
        [SerializeField] private Color _startColor = Color.white;
        [SerializeField] private Color _endColor = new Color(1f, 1f, 1f, 0f);
        [SerializeField] private float _startScale = 0.8f;
        [SerializeField] private float _endScale = 1.2f;
        [SerializeField] private AnimationCurve _fadeCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

        private SpriteRenderer _spriteRenderer;
        private float _timer;
        private Vector3 _initialScale;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            _initialScale = transform.localScale;
        }

        private void Start()
        {
            // Set initial state
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = _startColor;
            }
            transform.localScale = _initialScale * _startScale;

            // Schedule destruction
            Destroy(gameObject, _lifetime);
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            float t = Mathf.Clamp01(_timer / _lifetime);
            float curveValue = _fadeCurve.Evaluate(t);

            // Fade color
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = Color.Lerp(_startColor, _endColor, t);
            }

            // Scale animation
            float scale = Mathf.Lerp(_startScale, _endScale, t);
            transform.localScale = _initialScale * scale;
        }

        /// <summary>
        /// Initialize the slash effect with custom settings.
        /// </summary>
        public void Initialize(float lifetime, Color color)
        {
            _lifetime = lifetime;
            _startColor = color;
            _endColor = new Color(color.r, color.g, color.b, 0f);
            
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = _startColor;
            }

            // Re-schedule destruction with new lifetime
            CancelInvoke();
            Destroy(gameObject, _lifetime);
        }
    }
}
