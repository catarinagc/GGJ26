using UnityEngine;

namespace Boss
{
    /// <summary>
    /// Visual effects for boss projectiles - handles pulsing, glowing, and particle effects
    /// </summary>
    public class BossVisualEffects : MonoBehaviour
    {
        public enum EffectType
        {
            Wave,       // Noble Slashes - Purple vertical waves
            Falling,    // Falling Grace - Red falling orbs
            Telegraph,  // Precision Strike warning
            Strike      // Precision Strike explosion
        }

        [Header("Effect Type")]
        [SerializeField] private EffectType _effectType = EffectType.Wave;

        [Header("Pulse Settings")]
        [SerializeField] private bool _enablePulse = true;
        [SerializeField] private float _pulseSpeed = 4f;
        [SerializeField] private float _pulseMinScale = 0.9f;
        [SerializeField] private float _pulseMaxScale = 1.1f;

        [Header("Glow Settings")]
        [SerializeField] private bool _enableGlow = true;
        [SerializeField] private float _glowSpeed = 3f;
        [SerializeField] private float _glowMinAlpha = 0.7f;
        [SerializeField] private float _glowMaxAlpha = 1f;
        [SerializeField] private Color _glowColor = Color.white;

        [Header("Rotation Settings")]
        [SerializeField] private bool _enableRotation = false;
        [SerializeField] private float _rotationSpeed = 180f;

        [Header("Trail Settings")]
        [SerializeField] private bool _createTrail = true;
        [SerializeField] private float _trailTime = 0.3f;
        [SerializeField] private float _trailStartWidth = 1f;
        [SerializeField] private float _trailEndWidth = 0f;

        [Header("Particle Settings")]
        [SerializeField] private bool _createParticles = true;
        [SerializeField] private int _particleCount = 20;
        [SerializeField] private float _particleLifetime = 0.5f;

        [Header("Telegraph Specific")]
        [SerializeField] private bool _telegraphPulse = true;
        [SerializeField] private float _telegraphExpandSpeed = 2f;

        private SpriteRenderer _spriteRenderer;
        private TrailRenderer _trailRenderer;
        private ParticleSystem _particleSystem;
        private Vector3 _originalScale;
        private Color _originalColor;
        private float _timeOffset;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _originalScale = transform.localScale;
            _timeOffset = Random.Range(0f, Mathf.PI * 2f);

            if (_spriteRenderer != null)
            {
                _originalColor = _spriteRenderer.color;
                
                // Auto-detect effect type based on color
                if (_originalColor.b > 0.8f && _originalColor.r > 0.5f) // Purple
                {
                    _effectType = EffectType.Wave;
                }
                else if (_originalColor.r > 0.8f && _originalColor.g < 0.5f && _originalColor.b < 0.5f) // Red
                {
                    _effectType = EffectType.Falling;
                }
            }

            SetupEffectByType();
        }

        private void Start()
        {
            if (_createTrail && _trailRenderer == null)
            {
                CreateTrailRenderer();
            }

            if (_createParticles && _particleSystem == null)
            {
                CreateParticleSystem();
            }
        }

        private void SetupEffectByType()
        {
            switch (_effectType)
            {
                case EffectType.Wave:
                    SetupWaveEffect();
                    break;
                case EffectType.Falling:
                    SetupFallingEffect();
                    break;
                case EffectType.Telegraph:
                    SetupTelegraphEffect();
                    break;
                case EffectType.Strike:
                    SetupStrikeEffect();
                    break;
            }
        }

        private void SetupWaveEffect()
        {
            // Purple glowing wave
            _enablePulse = true;
            _enableGlow = true;
            _enableRotation = false;
            _createTrail = true;
            _createParticles = true;
            _glowColor = new Color(0.8f, 0.2f, 1f, 1f); // Bright purple
            _pulseSpeed = 6f;
            _pulseMinScale = 0.95f;
            _pulseMaxScale = 1.05f;
            _trailTime = 0.4f;
            _trailStartWidth = 2f;
        }

        private void SetupFallingEffect()
        {
            // Red glowing orb
            _enablePulse = true;
            _enableGlow = true;
            _enableRotation = true;
            _createTrail = true;
            _createParticles = true;
            _glowColor = new Color(1f, 0.3f, 0.3f, 1f); // Bright red
            _pulseSpeed = 8f;
            _pulseMinScale = 0.85f;
            _pulseMaxScale = 1.15f;
            _rotationSpeed = 360f;
            _trailTime = 0.5f;
            _trailStartWidth = 1.5f;
        }

        private void SetupTelegraphEffect()
        {
            // Red warning circle that pulses
            _enablePulse = false;
            _enableGlow = true;
            _enableRotation = false;
            _createTrail = false;
            _createParticles = false;
            _telegraphPulse = true;
            _glowColor = new Color(1f, 0f, 0f, 0.6f); // Semi-transparent red
            _glowSpeed = 8f;
            _glowMinAlpha = 0.3f;
            _glowMaxAlpha = 0.8f;
        }

        private void SetupStrikeEffect()
        {
            // Yellow explosion
            _enablePulse = false;
            _enableGlow = true;
            _enableRotation = false;
            _createTrail = false;
            _createParticles = true;
            _glowColor = new Color(1f, 1f, 0f, 1f); // Bright yellow
            _particleCount = 50;
            _particleLifetime = 0.8f;

            // Start explosion animation
            StartCoroutine(ExplosionAnimation());
        }

        private void Update()
        {
            float time = Time.time + _timeOffset;

            if (_enablePulse)
            {
                ApplyPulse(time);
            }

            if (_enableGlow && _spriteRenderer != null)
            {
                ApplyGlow(time);
            }

            if (_enableRotation)
            {
                ApplyRotation();
            }

            if (_telegraphPulse && _effectType == EffectType.Telegraph)
            {
                ApplyTelegraphPulse(time);
            }
        }

        private void ApplyPulse(float time)
        {
            float pulse = Mathf.Lerp(_pulseMinScale, _pulseMaxScale, (Mathf.Sin(time * _pulseSpeed) + 1f) * 0.5f);
            transform.localScale = _originalScale * pulse;
        }

        private void ApplyGlow(float time)
        {
            float alpha = Mathf.Lerp(_glowMinAlpha, _glowMaxAlpha, (Mathf.Sin(time * _glowSpeed) + 1f) * 0.5f);
            Color newColor = _glowColor;
            newColor.a = alpha;
            _spriteRenderer.color = newColor;
        }

        private void ApplyRotation()
        {
            transform.Rotate(0f, 0f, _rotationSpeed * Time.deltaTime);
        }

        private void ApplyTelegraphPulse(float time)
        {
            // Expanding ring effect
            float expand = 1f + Mathf.Sin(time * _telegraphExpandSpeed) * 0.1f;
            transform.localScale = _originalScale * expand;
        }

        private void CreateTrailRenderer()
        {
            _trailRenderer = gameObject.AddComponent<TrailRenderer>();
            _trailRenderer.time = _trailTime;
            _trailRenderer.startWidth = _trailStartWidth;
            _trailRenderer.endWidth = _trailEndWidth;
            _trailRenderer.material = new Material(Shader.Find("Sprites/Default"));

            // Set trail color based on effect type
            Color trailColor = _glowColor;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(trailColor, 0f),
                    new GradientColorKey(trailColor, 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            _trailRenderer.colorGradient = gradient;
            _trailRenderer.sortingOrder = _spriteRenderer != null ? _spriteRenderer.sortingOrder - 1 : 0;
        }

        private void CreateParticleSystem()
        {
            GameObject particleObj = new GameObject("Particles");
            particleObj.transform.SetParent(transform);
            particleObj.transform.localPosition = Vector3.zero;

            _particleSystem = particleObj.AddComponent<ParticleSystem>();

            var main = _particleSystem.main;
            main.startLifetime = _particleLifetime;
            main.startSpeed = 2f;
            main.startSize = 0.3f;
            main.startColor = _glowColor;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = _particleCount * 2;

            var emission = _particleSystem.emission;
            emission.rateOverTime = _particleCount;

            var shape = _particleSystem.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.5f;

            var colorOverLifetime = _particleSystem.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(_glowColor, 0f),
                    new GradientColorKey(_glowColor, 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = gradient;

            var sizeOverLifetime = _particleSystem.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(1f, 0f)
            ));

            // Set renderer
            var renderer = particleObj.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Sprites/Default"));
            renderer.sortingOrder = _spriteRenderer != null ? _spriteRenderer.sortingOrder + 1 : 1;
        }

        private System.Collections.IEnumerator ExplosionAnimation()
        {
            float duration = 0.5f;
            float elapsed = 0f;
            Vector3 startScale = transform.localScale;
            Vector3 endScale = startScale * 2f;

            // Burst particles
            if (_particleSystem != null)
            {
                _particleSystem.Emit(_particleCount);
            }

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Expand and fade
                transform.localScale = Vector3.Lerp(startScale, endScale, t);

                if (_spriteRenderer != null)
                {
                    Color c = _spriteRenderer.color;
                    c.a = Mathf.Lerp(1f, 0f, t);
                    _spriteRenderer.color = c;
                }

                yield return null;
            }
        }

        /// <summary>
        /// Set the effect type at runtime
        /// </summary>
        public void SetEffectType(EffectType type)
        {
            _effectType = type;
            SetupEffectByType();
        }

        /// <summary>
        /// Set custom glow color
        /// </summary>
        public void SetGlowColor(Color color)
        {
            _glowColor = color;
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = color;
            }
        }
    }
}
