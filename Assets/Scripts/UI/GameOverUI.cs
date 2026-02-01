using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Combat;

namespace UI
{
    /// <summary>
    /// Game Over screen that activates when the player dies.
    /// Provides a Retry button to reload the current scene.
    /// </summary>
    public class GameOverUI : MonoBehaviour
    {

        private AudioManager _audioManager;
        [Header("References")]
        [SerializeField] private Health _playerHealth;
        [SerializeField] private GameObject _gameOverPanel;
        [SerializeField] private Button _retryButton;
        [SerializeField] private Text _gameOverText;

        [Header("Settings")]
        [SerializeField] private float _showDelay = 0.5f;
        [SerializeField] private bool _pauseGameOnDeath = true;

        [Header("Animation")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _fadeInSpeed = 2f;

        private bool _isGameOver;
        private float _fadeTimer;

        private void Awake()
        {

            _audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();
            if (_audioManager == null)
            {
                Debug.LogError("AudioManager not found in scene!");
            }
            // Auto-find player health if not assigned
            if (_playerHealth == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player == null)
                {
                    player = GameObject.Find("Player");
                }

                if (player != null)
                {
                    _playerHealth = player.GetComponent<Health>();
                }
            }

            // Hide game over panel initially
            if (_gameOverPanel != null)
            {
                _gameOverPanel.SetActive(false);
            }

            // Setup retry button
            if (_retryButton != null)
            {
                _retryButton.onClick.AddListener(OnRetryClicked);
            }

            // Initialize canvas group
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }
        }

        private void OnEnable()
        {
            if (_playerHealth != null)
            {
                _playerHealth.OnDeath += OnPlayerDeath;
            }
        }

        private void OnDisable()
        {
            if (_playerHealth != null)
            {
                _playerHealth.OnDeath -= OnPlayerDeath;
            }
        }

        private void Update()
        {
            // Fade in animation
            if (_isGameOver && _canvasGroup != null && _canvasGroup.alpha < 1f)
            {
                _canvasGroup.alpha = Mathf.MoveTowards(_canvasGroup.alpha, 1f, Time.unscaledDeltaTime * _fadeInSpeed);
            }
        }

        private void OnPlayerDeath()
        {
            if (_isGameOver) return;

            _isGameOver = true;
            _audioManager.PlaySFX(_audioManager.death);
            Invoke(nameof(ShowGameOver), _showDelay);
        }

        //private void ShowGameOver()
        //{
        //    if (_gameOverPanel != null)
        //    {
        //        _gameOverPanel.SetActive(true);
        //    }

        //    if (_pauseGameOnDeath)
        //    {
        //        Time.timeScale = 0f;
        //    }

        //    Debug.Log("[GameOverUI] Game Over!");
        //}

        private void ShowGameOver()
        {
            if (_gameOverPanel != null)
            {
                _gameOverPanel.SetActive(true);
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
            }

            if (_pauseGameOnDeath)
            {
                Time.timeScale = 0f;
            }

            Debug.Log("[GameOverUI] Game Over!");
        }


        private void OnRetryClicked()
        {
            Debug.Log("[GameOverUI] Retry button CLICKED");

            Time.timeScale = 1f;
            string currentScene = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(currentScene);
        }


        /// <summary>
        /// Manually trigger game over (for testing).
        /// </summary>
        public void TriggerGameOver()
        {
            OnPlayerDeath();
        }

        /// <summary>
        /// Manually set the player health reference.
        /// </summary>
        public void SetPlayerHealth(Health health)
        {
            if (_playerHealth != null)
            {
                _playerHealth.OnDeath -= OnPlayerDeath;
            }

            _playerHealth = health;

            if (_playerHealth != null)
            {
                _playerHealth.OnDeath += OnPlayerDeath;
            }
        }

        private void OnDestroy()
        {
            // Ensure time scale is reset
            Time.timeScale = 1f;

            if (_retryButton != null)
            {
                _retryButton.onClick.RemoveListener(OnRetryClicked);
            }
        }
    }
}
