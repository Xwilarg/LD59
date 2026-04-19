using UnityEngine;

namespace LD59.Manager
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { private set; get; }

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
    }
}
