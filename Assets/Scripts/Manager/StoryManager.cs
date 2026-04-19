using LD59.SO;
using LD59.VN;
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
        private Button _junctionBtn, _signalBtn;

        [Header("Timetable")]
        [SerializeField]
        public Transform _timetableContainer;
        [SerializeField]
        public GameObject _timetablePrefab;

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
                for (int i = _timetableContainer.childCount - 1; i >= 0; i--) Destroy(_timetableContainer.GetChild(i).gameObject);
            });

            GameStateManager.Instance.OnReset.AddListener(() => { _storyIndex--; });
        }

        private void Update()
        {
            if (!_isPlaying) return;

            _timer += Time.deltaTime;
            foreach (var t in _trains)
            {
                if (t.Status == TrainStatus.Waiting && _timer >= t.Info.DepartureTime - 10f)
                {
                    t.Status = TrainStatus.Announced;
                    var go = Instantiate(_timetablePrefab, _timetableContainer);
                    t.Annoucement = go.GetComponent<TrainPopup>();
                    t.Annoucement.SetLabel(t.Label);
                    t.Annoucement.SetDescription($"Departure announced from {MapManager.Instance.ColorNameWithStation(t.Info.From, MapManager.StationToName(t.Info.From))}");
                    WarningManager.Instance.ShowWarning($"{t.PlainLabel} is leaving in 10 seconds from {MapManager.StationToName(t.Info.From)} to {MapManager.StationToName(t.Info.To)}");
                }
                else if (t.Status == TrainStatus.Announced && _timer >= t.Info.DepartureTime)
                {
                    t.Status = TrainStatus.Launched;
                    MapManager.Instance.SpawnTrain(t.Info.From, t.Info.To, t.Label, t.PlainLabel);
                    t.Annoucement.SetDescription($"From {MapManager.Instance.ColorNameWithStation(t.Info.From, MapManager.StationToName(t.Info.From))}\nTo {MapManager.Instance.ColorNameWithStation(t.Info.To, MapManager.StationToName(t.Info.To))}");
                    WarningManager.Instance.ShowWarning($"{t.PlainLabel} is leaving from {MapManager.StationToName(t.Info.From)} to {MapManager.StationToName(t.Info.To)}");
                }
            }
        }

        public void ShowNextStory()
        {
            VNManager.Instance.ShowStory(_stories[_storyIndex].Intro, onDone: () =>
            {
                _readyUp.SetActive(true);

                var story = _stories[_storyIndex];
                if (story.FreeJunctions)
                {
                    CanUseJunctions = true;
                    _junctionBtn.interactable = true;
                }
                if (story.FreeSignals)
                {
                    _signalBtn.interactable = true;
                }

                int id = 1;
                foreach (var t in story.Trains)
                {
                    var label = $"{MapManager.Instance.ColorNameWithStation(t.From, t.From.ToString()[0].ToString())}{MapManager.Instance.ColorNameWithStation(t.To, t.To.ToString()[0].ToString())}{id:X2}";
                    _trains.Add(new()
                    {
                        Status = TrainStatus.Waiting,
                        Info = t,
                        PlainLabel = $"{t.From.ToString()[0]}{t.To.ToString()[0]}{id:X2}",
                        Label = label
                    });
                    id++;
                }

                MapManager.Instance.SetupStations(story.Trains.Select(x => x.From), story.Trains.Select(x => x.To));
            });
        }

        public void JumpToChapter(int index)
        {
            GameStateManager.Instance.OnReset.Invoke();
            _storyIndex = index;
            ShowNextStory();
        }

        public void LaunchTrains()
        {
            _timer = 0f;
            _isPlaying = true;
            _storyIndex++;

            _readyUp.SetActive(false);
        }

        public void ArriveTrain(string label)
        {
            var t = _trains.First(x => x.Label == label);
            t.Status = TrainStatus.Arrived;
            Destroy(t.Annoucement.gameObject);

            if (_trains.All(x => x.Status == TrainStatus.Arrived))
            {
                _storyIndex++;
                GameStateManager.Instance.OnReset.Invoke();
                ShowNextStory();
            }
        }
    }

    public class TrainTime
    {
        public TrainStatus Status;
        public StoryTrain Info;
        public string PlainLabel;
        public string Label;

        public TrainPopup Annoucement;
    }

    public enum TrainStatus
    {
        Waiting,
        Announced,
        Launched,
        Arrived
    }
}
