using UnityEngine;
using UnityEngine.Events;

namespace LD59.Manager
{
    public class GameStateManager : MonoBehaviour
    {
        public static GameStateManager Instance { private set; get; }

        private void Awake()
        {
            Instance = this;
        }

        public bool IsGameOver { private set; get; }

        public UnityEvent OnReset { get; } = new();
        private float _looseTimer;

        public void Loose(string reason)
        {
            if (IsGameOver) return;

            IsGameOver = true;
            WarningManager.Instance.ShowWarning($"An accident happened: {reason}");
            _looseTimer = 1f;
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
