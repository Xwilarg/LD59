using TMPro;
using UnityEngine;

namespace LD59.Manager
{
    public class WarningManager : MonoBehaviour
    {
        public static WarningManager Instance { private set; get; }

        [SerializeField]
        private TMP_Text _warning;

        private float _timer;

        private void Awake()
        {
            Instance = this;
            _warning.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (_timer > 0f)
            {
                _timer -= Time.deltaTime;
                if (_timer <= 0f)
                {
                    _warning.gameObject.SetActive(false);
                }
            }
        }

        public void ShowWarning(string text)
        {
            _warning.gameObject.SetActive(true);
            _warning.text = text;
            _timer = 3f;
        }
    }
}
