using UnityEngine;
using UnityEngine.SceneManagement;

namespace LD59.Manager
{
    public class SettingsManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject _menu;

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
    }
}
