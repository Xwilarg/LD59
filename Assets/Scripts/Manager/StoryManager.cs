using LD59.SO;
using UnityEngine;

namespace LD59.Manager
{
    public class StoryManager : MonoBehaviour
    {
        public static StoryManager Instance { private set; get; }

        [SerializeField]
        private StoryLevelInfo[] _stories;
        private int _storyIndex;

        private void Awake()
        {
            Instance = this;
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
        }
    }
}
