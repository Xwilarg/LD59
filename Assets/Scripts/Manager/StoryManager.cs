using LD59.SO;
using Sketch.VN;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

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

        [SerializeField]
        private Button _signalBtn;

        private float _timer;
        private bool _isPlaying = false;

        public bool CanUseJunctions { private set; get; }
        public bool CanUseSignals { private set; get; }

        private readonly List<TrainTime> _trains = new();

        private void Awake()
        {
            Instance = this;

            _readyUp.SetActive(false);
        }

        private void Start()
        {
            ShowNextStory();

            GameStateManager.Instance.OnReset.AddListener(() =>
            {
                _trains.Clear();
                _readyUp.SetActive(true);
                _isPlaying = false;
            });

            GameStateManager.Instance.OnReset.AddListener(() => { _storyIndex--; });
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            foreach (var t in _trains)
            {
                if (t.Status == TrainStatus.Waiting && _timer >= t.Info.DepartureTime - 10f)
                {
                    t.Status = TrainStatus.Announced;
                    WarningManager.Instance.ShowWarning($"{t.Label} is leaving in 10 seconds from {MapManager.StationToName(t.Info.From)} to {MapManager.StationToName(t.Info.To)}");
                }
                else if (t.Status == TrainStatus.Announced && _timer >= t.Info.DepartureTime)
                {
                    t.Status = TrainStatus.Launched;
                    MapManager.Instance.SpawnTrain(t.Info.From, t.Info.To, t.Label);
                    WarningManager.Instance.ShowWarning($"{t.Label} is leaving from {MapManager.StationToName(t.Info.From)} to {MapManager.StationToName(t.Info.To)}");
                }
            }
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
            _timer = 0f;
            _isPlaying = true;

            var story = _stories[_storyIndex];
            int id = 1;
            foreach (var t in story.Trains)
            {
                var label = $"{t.From.ToString()[0]}{t.To.ToString()[0]}{id:00}";
                _trains.Add(new()
                {
                    Status = TrainStatus.Waiting,
                    Info = t,
                    Label = label
                });
                id++;
            }
            if (story.FreeJunctions)
            {
                CanUseJunctions = true;
                _signalBtn.interactable = true;
            }
            if (story.FreeSignals)
            {
                _signalBtn.interactable = true;
            }
            _storyIndex++;

            _readyUp.SetActive(false);
        }

        public void ArriveTrain(string label)
        {
            _trains.First(x => x.Label == label).Status = TrainStatus.Arrived;
            ShowNextStory();
        }
    }

    public class TrainTime
    {
        public TrainStatus Status;
        public StoryTrain Info;
        public string Label;
    }

    public enum TrainStatus
    {
        Waiting,
        Announced,
        Launched,
        Arrived
    }
}
