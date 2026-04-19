using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LD59.Manager
{
    public class SettingsManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject _menu;

        [SerializeField]
        private Button _allowPause;

        private void Awake()
        {
            _menu.SetActive(false);
        }

        public void ToggleMenu()
        {
            _menu.SetActive(!_menu.activeInHierarchy);
        }

        public void BackToMainMenu()
        {
            SceneManager.LoadScene("Menu");
        }

        public void AllowPause()
        {
            _allowPause.interactable = false;
            GameStateManager.Instance.IsPausedAllowed = true;
        }
    }
}
