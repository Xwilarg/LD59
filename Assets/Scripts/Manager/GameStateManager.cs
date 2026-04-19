using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace LD59.Manager
{
    public class GameStateManager : MonoBehaviour
    {
        public static GameStateManager Instance { private set; get; }

        [SerializeField]
        private GameObject _pauseText;

        public bool IsPausedAllowed { set; get; }

        private void Awake()
        {
            Instance = this;

            if (_pauseText != null) _pauseText.SetActive(false);
        }

        public bool IsGameOver { private set; get; }

        public UnityEvent OnReset { get; } = new();
        private float _looseTimer;

        private bool _isPaused;

        public void Loose(string reason)
        {
            if (IsGameOver) return;

            IsGameOver = true;
            WarningManager.Instance.ShowWarning($"An accident happened: {reason}", true);
            _looseTimer = 1f;
        }

        public void TogglePause(InputAction.CallbackContext value)
        {
            if (value.phase == InputActionPhase.Started && IsPausedAllowed)
            {
                _isPaused = !_isPaused;
                Time.timeScale = _isPaused ? 0f : 1f;
                _pauseText.SetActive(_isPaused);
            }
        }

        private void Update()
        {
            if (!IsGameOver) return;

            _looseTimer -= Time.deltaTime * (1f / 3f);
            // TODO: break time deltatime
            //Time.timeScale = Mathf.Lerp(0f, 1f, _looseTimer);

            if (_looseTimer <= 0f)
            {
                IsGameOver = false;
                Time.timeScale = 1f;
                OnReset.Invoke();
            }
        }
    }
}
