using UnityEngine;

namespace LD59.Manager
{
    public class StoryManager : MonoBehaviour
    {
        public static StoryManager Instance { private set; get; }


        private int _storyIndex;
        private StoryElement[] _stories = new[]
        {
            new StoryElement()
            {
                Trains = new StoryTrain[]
                {
                    new()
                    {
                        From = Station.Arieta,
                        To = Station.Sorena
                    }
                }
            }
        };

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

    public record StoryElement
    {
        public StoryTrain[] Trains;
    }

    public record StoryTrain
    {
        public Station From;
        public Station To;
    }
}
