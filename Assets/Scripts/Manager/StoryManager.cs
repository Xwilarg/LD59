using LD59.SO;
using Sketch.VN;
using UnityEngine;

namespace LD59.Manager
{
    public class StoryManager : MonoBehaviour
    {
        public static StoryManager Instance { private set; get; }

        [SerializeField]
        private StoryLevelInfo[] _stories;
        private int _storyIndex;

        [SerializeField]
        private GameObject _readyUp;

        private void Awake()
        {
            Instance = this;

            _readyUp.SetActive(false);
        }

        private void Start()
        {
            ShowNextStory();
        }

        public void ShowNextStory()
        {
            VNManager.Instance.ShowStory(_stories[_storyIndex].Intro, onDone: () =>
            {
                _readyUp.SetActive(true);
            });
        }

        public void LaunchTrains()
        {
            int id = 0;
            foreach (var t in _stories[_storyIndex].Trains)
            {
                var label = $"{t.From.ToString()[0]}{t.To.ToString()[0]}{id:00}";
                MapManager.Instance.SpawnTrain(t.From, t.To, label);
                id++;
            }
            _storyIndex++;

            _readyUp.SetActive(false);
        }
    }
}
