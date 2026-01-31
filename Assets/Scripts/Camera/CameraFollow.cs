using UnityEngine;

namespace CameraSystem
{
    /// <summary>
    /// Smooth camera follow script for 2D platformers.
    /// Follows the target with configurable smoothing and offset.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform _target;
        [SerializeField] private bool _findPlayerOnStart = true;

        [Header("Follow Settings")]
        [SerializeField] private float _smoothSpeed = 0.125f;
        [SerializeField] private Vector3 _offset = new Vector3(0f, 0f, -10f);

        [Header("Bounds (Optional)")]
        [SerializeField] private bool _useBounds = false;
        [SerializeField] private float _minX = -10f;
        [SerializeField] private float _maxX = 10f;
        [SerializeField] private float _minY = -5f;
        [SerializeField] private float _maxY = 5f;

        [Header("Look Ahead")]
        [SerializeField] private bool _useLookAhead = false;
        [SerializeField] private float _lookAheadDistance = 2f;
        [SerializeField] private float _lookAheadSpeed = 0.5f;

        private Vector3 _currentVelocity;
        private float _currentLookAhead;
        private float _targetLookAhead;

        private void Start()
        {
            if (_findPlayerOnStart && _target == null)
            {
                GameObject player = GameObject.Find("Player");
                if (player != null)
                {
                    _target = player.transform;
                }
                else
                {
                    Debug.LogWarning("[CameraFollow] Player not found in scene!");
                }
            }

            // Initialize position
            if (_target != null)
            {
                Vector3 targetPos = _target.position + _offset;
                targetPos.z = _offset.z;
                transform.position = targetPos;
            }
        }

        private void LateUpdate()
        {
            if (_target == null) return;

            // Calculate target position
            Vector3 desiredPosition = _target.position + _offset;

            // Apply look ahead based on target's facing direction
            if (_useLookAhead)
            {
                _targetLookAhead = _target.localScale.x >= 0 ? _lookAheadDistance : -_lookAheadDistance;
                _currentLookAhead = Mathf.Lerp(_currentLookAhead, _targetLookAhead, _lookAheadSpeed * Time.deltaTime);
                desiredPosition.x += _currentLookAhead;
            }

            // Ensure Z offset is maintained
            desiredPosition.z = _offset.z;

            // Apply bounds if enabled
            if (_useBounds)
            {
                desiredPosition.x = Mathf.Clamp(desiredPosition.x, _minX, _maxX);
                desiredPosition.y = Mathf.Clamp(desiredPosition.y, _minY, _maxY);
            }

            // Smooth follow using SmoothDamp for more natural movement
            Vector3 smoothedPosition = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref _currentVelocity,
                _smoothSpeed
            );

            // Maintain Z position
            smoothedPosition.z = _offset.z;

            transform.position = smoothedPosition;
        }

        /// <summary>
        /// Set a new target for the camera to follow.
        /// </summary>
        public void SetTarget(Transform newTarget)
        {
            _target = newTarget;
        }

        /// <summary>
        /// Instantly snap camera to target position.
        /// </summary>
        public void SnapToTarget()
        {
            if (_target == null) return;

            Vector3 targetPos = _target.position + _offset;
            targetPos.z = _offset.z;
            transform.position = targetPos;
            _currentVelocity = Vector3.zero;
        }

        /// <summary>
        /// Set camera bounds.
        /// </summary>
        public void SetBounds(float minX, float maxX, float minY, float maxY)
        {
            _minX = minX;
            _maxX = maxX;
            _minY = minY;
            _maxY = maxY;
            _useBounds = true;
        }

        private void OnDrawGizmosSelected()
        {
            if (!_useBounds) return;

            Gizmos.color = Color.yellow;
            Vector3 center = new Vector3((_minX + _maxX) / 2f, (_minY + _maxY) / 2f, 0f);
            Vector3 size = new Vector3(_maxX - _minX, _maxY - _minY, 0.1f);
            Gizmos.DrawWireCube(center, size);
        }
    }
}
