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
        private Transform _timetableContainer;
        [SerializeField]
        private GameObject _timetablePrefab;

        [SerializeField]
        private TextAsset _outro;

        private float _timer;
        private bool _isPlaying = false;

        public bool CanUseJunctions { private set; get; }
        public bool CanUseSignals { private set; get; }

        private readonly List<TrainTime> _trains = new();

        public float TotalTime { private set; get; }
        public float _levelRefTime;

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
                _storyIndex--;

                _trains.Clear();
                _readyUp.SetActive(true);
                _isPlaying = false;
                for (int i = _timetableContainer.childCount - 1; i >= 0; i--) Destroy(_timetableContainer.GetChild(i).gameObject);

                SetupTrains();
            });
        }

        private void Update()
        {
            if (!_isPlaying) return;

            _timer += Time.deltaTime;
            foreach (var t in _trains) // Deal with most far away trains first, for timer management reasons
            {
                if (t.Status == TrainStatus.Waiting && _timer >= t.Info.DepartureTime - 10f)
                {
                    t.Status = TrainStatus.Announced;
                    var go = Instantiate(_timetablePrefab, _timetableContainer);
                    t.Annoucement = go.GetComponent<TrainPopup>();
                    t.Annoucement.SetLabel(t.Label);
                    t.Annoucement.SetDescription($"Departure announced from {MapManager.Instance.ColorNameWithStation(t.Info.From, MapManager.StationToName(t.Info.From))} to {MapManager.Instance.ColorNameWithStation(t.Info.To, MapManager.StationToName(t.Info.To))}");
                    WarningManager.Instance.ShowWarning($"{t.PlainLabel} is leaving in 10 seconds from {MapManager.StationToName(t.Info.From)} to {MapManager.StationToName(t.Info.To)}");
                }
                else if (t.Status == TrainStatus.Announced)
                {
                    if (_timer >= t.Info.DepartureTime)
                    {
                        t.Status = TrainStatus.Launched;
                        t.Platform.AssociatedRail.ResetTimer();
                        MapManager.Instance.SpawnTrain(t.Info.From, t.Info.To, t.Label, t.PlainLabel, t.Info.Type);
                        t.Annoucement.SetDescription($"From {MapManager.Instance.ColorNameWithStation(t.Info.From, MapManager.StationToName(t.Info.From))}\nTo {MapManager.Instance.ColorNameWithStation(t.Info.To, MapManager.StationToName(t.Info.To))}");
                        WarningManager.Instance.ShowWarning($"{t.PlainLabel} is leaving from {MapManager.StationToName(t.Info.From)} to {MapManager.StationToName(t.Info.To)}");
                    }
                    else
                    {
                        t.Platform.AssociatedRail.SetTimer(t.Info.DepartureTime - _timer);
                    }
                }
            }
        }

        public void ShowNextStory()
        {
            if (_storyIndex >= _stories.Length)
            {
                VNManager.Instance.ShowStory(_outro, onDone: () => {}, updateVariables: (state) =>
                {
                    state["minutes"] = Mathf.RoundToInt(TotalTime / 60f);
                    state["seconds"] = (int)(TotalTime % 60);
                });
                return;
            }

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

                SetupTrains();
            });
        }

        private string TypeToString(TrainType type)
        {
            return type switch
            {
                TrainType.Normal => "p",
                TrainType.Commercial => "c",
                TrainType.HighSpeed => "h",
                _ => throw new System.NotImplementedException()
            };
        }

        private Color TypeToColor(TrainType type)
        {
            return type switch
            {
                TrainType.Normal => Color.blue,
                TrainType.Commercial => Color.orange,
                TrainType.HighSpeed => Color.red,
                _ => throw new System.NotImplementedException()
            };
        }

        private void SetupTrains()
        {
            if (_storyIndex >= _stories.Length) return;

            var story = _stories[_storyIndex];
            int id = 1;
            _trains.Clear();
            foreach (var t in story.Trains)
            {
                var label = $"{MapManager.Instance.ColorWith(TypeToColor(t.Type), TypeToString(t.Type))}{MapManager.Instance.ColorNameWithStation(t.From, t.From.ToString()[0].ToString())}{MapManager.Instance.ColorNameWithStation(t.To, t.To.ToString()[0].ToString())}{id:X2}";
                _trains.Add(new()
                {
                    Status = TrainStatus.Waiting,
                    Info = t,
                    PlainLabel = $"{TypeToString(t.Type)}{t.From.ToString()[0]}{t.To.ToString()[0]}{id:X2}",
                    Label = label,
                    Platform = MapManager.Instance.GetStationPlatform(t.From)
                });
                id++;
            }
            _trains.Reverse();

            MapManager.Instance.SetupStations(story.Trains.Select(x => x.From), story.Trains.Select(x => x.To));
        }

        public void JumpToChapter(int index)
        {
            _storyIndex = index + 1;
            GameStateManager.Instance.OnReset.Invoke();
            ShowNextStory();
        }

        public void ResetChapter()
        {
            if (!_isPlaying) _storyIndex++;
            GameStateManager.Instance.OnReset.Invoke();
            ShowNextStory();
        }

        public void LaunchTrains()
        {
            _timer = 0f;
            _isPlaying = true;
            _storyIndex++;
            _levelRefTime = Time.unscaledTime;

            _readyUp.SetActive(false);
        }

        public void ArriveTrain(string label)
        {
            var t = _trains.First(x => x.Label == label);
            t.Status = TrainStatus.Arrived;
            Destroy(t.Annoucement.gameObject);

            if (_trains.All(x => x.Status == TrainStatus.Arrived))
            {
                TotalTime += Time.unscaledTime - _levelRefTime;
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

        public Platform Platform;

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
