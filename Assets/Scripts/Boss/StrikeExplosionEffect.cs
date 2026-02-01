using UnityEngine;
using System.Collections;

namespace Boss
{
    /// <summary>
    /// Explosion effect for Precision Strike - creates a dramatic yellow/orange explosion
    /// </summary>
    public class StrikeExplosionEffect : MonoBehaviour
    {
        [Header("Visual Settings")]
        [SerializeField] private Color _coreColor = new Color(1f, 1f, 0.5f, 1f); // Bright yellow
        [SerializeField] private Color _outerColor = new Color(1f, 0.6f, 0f, 0.8f); // Orange
        [SerializeField] private float _explosionDuration = 0.5f;
        [SerializeField] private float _maxScale = 3f;

        [Header("Particle Settings")]
        [SerializeField] private int _sparkCount = 30;
        [SerializeField] private float _sparkSpeed = 15f;

        private SpriteRenderer _coreRenderer;
        private SpriteRenderer _outerRenderer;
        private SpriteRenderer _flashRenderer;
        private ParticleSystem _sparkParticles;

        private void Awake()
        {
            _coreRenderer = GetComponent<SpriteRenderer>();
            if (_coreRenderer == null)
            {
                _coreRenderer = gameObject.AddComponent<SpriteRenderer>();
            }

            CreateExplosionLayers();
            CreateSparkParticles();
        }

        private void Start()
        {
            StartCoroutine(PlayExplosion());
        }

        private void CreateExplosionLayers()
        {
            // Outer glow layer
            GameObject outer = new GameObject("OuterGlow");
            outer.transform.SetParent(transform);
            outer.transform.localPosition = Vector3.zero;
            outer.transform.localScale = Vector3.one * 1.5f;

            _outerRenderer = outer.AddComponent<SpriteRenderer>();
            _outerRenderer.sprite = _coreRenderer.sprite;
            _outerRenderer.color = _outerColor;
            _outerRenderer.sortingOrder = _coreRenderer.sortingOrder - 1;

            // Flash layer (bright white)
            GameObject flash = new GameObject("Flash");
            flash.transform.SetParent(transform);
            flash.transform.localPosition = Vector3.zero;
            flash.transform.localScale = Vector3.one * 2f;

            _flashRenderer = flash.AddComponent<SpriteRenderer>();
            _flashRenderer.sprite = _coreRenderer.sprite;
            _flashRenderer.color = new Color(1f, 1f, 1f, 0.8f);
            _flashRenderer.sortingOrder = _coreRenderer.sortingOrder + 1;

            // Set core color
            _coreRenderer.color = _coreColor;
        }

        private void CreateSparkParticles()
        {
            GameObject particleObj = new GameObject("Sparks");
            particleObj.transform.SetParent(transform);
            particleObj.transform.localPosition = Vector3.zero;

            _sparkParticles = particleObj.AddComponent<ParticleSystem>();

            var main = _sparkParticles.main;
            main.startLifetime = 0.5f;
            main.startSpeed = _sparkSpeed;
            main.startSize = 0.4f;
            main.startColor = _coreColor;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = _sparkCount * 2;
            main.gravityModifier = 2f;

            var emission = _sparkParticles.emission;
            emission.enabled = false; // We'll burst manually

            var shape = _sparkParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.1f;

            var colorOverLifetime = _sparkParticles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(_coreColor, 0f),
                    new GradientColorKey(_outerColor, 0.5f),
                    new GradientColorKey(Color.red, 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0.8f, 0.5f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = gradient;

            var sizeOverLifetime = _sparkParticles.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(1f, 0f)
            ));

            // Set renderer
            var renderer = particleObj.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Sprites/Default"));
            renderer.sortingOrder = _coreRenderer.sortingOrder + 2;
        }

        private IEnumerator PlayExplosion()
        {
            // Emit sparks
            if (_sparkParticles != null)
            {
                _sparkParticles.Emit(_sparkCount);
            }

            float elapsed = 0f;
            Vector3 startScale = transform.localScale;
            Vector3 endScale = startScale * _maxScale;

            // Initial flash
            if (_flashRenderer != null)
            {
                _flashRenderer.transform.localScale = Vector3.one * 3f;
            }

            while (elapsed < _explosionDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / _explosionDuration;

                // Ease out expansion
                float easeOut = 1f - Mathf.Pow(1f - t, 2f);
                transform.localScale = Vector3.Lerp(startScale, endScale, easeOut);

                // Fade out
                float alpha = 1f - t;

                if (_coreRenderer != null)
                {
                    Color c = _coreColor;
                    c.a = alpha;
                    _coreRenderer.color = c;
                }

                if (_outerRenderer != null)
                {
                    Color c = _outerColor;
                    c.a = alpha * 0.8f;
                    _outerRenderer.color = c;
                    _outerRenderer.transform.localScale = Vector3.one * (1.5f + t * 0.5f);
                }

                if (_flashRenderer != null)
                {
                    Color c = Color.white;
                    c.a = Mathf.Lerp(0.8f, 0f, t * 2f); // Flash fades faster
                    _flashRenderer.color = c;
                    _flashRenderer.transform.localScale = Vector3.one * (2f + t * 2f);
                }

                yield return null;
            }

            // Wait for particles to finish
            yield return new WaitForSeconds(0.3f);

            Destroy(gameObject);
        }
    }
}
