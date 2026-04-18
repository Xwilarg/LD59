using UnityEngine;

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

        public void Loose(string reason)
        {
            if (IsGameOver) return;

            IsGameOver = true;
            WarningManager.Instance.ShowWarning($"An accident happened: {reason}");
        }
    }
}
