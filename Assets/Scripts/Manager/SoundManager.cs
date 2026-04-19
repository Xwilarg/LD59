using UnityEngine;

namespace LD59.Manager
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { private set; get; }

        private bool _isBgmActive = true;

        [SerializeField]
        private AudioSource _bgm;

        [SerializeField]
        private AudioSource _annoucement, _trackPlacement;

        private void Awake()
        {
            Instance = this;
        }

        public void PlayAnnoucement()
        {
            _annoucement.Play();
        }

        public void PlayTrackPlacement()
        {
            _trackPlacement.Play();
        }

        public void ToggleBGM()
        {
            _isBgmActive = !_isBgmActive;
            _bgm.volume = _isBgmActive ? 1f : 0f;
        }
    }
}
