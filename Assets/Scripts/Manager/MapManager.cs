using LD59.Map;
using LD59.SO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace LD59.Manager
{
    public class MapManager : MonoBehaviour
    {
        public static MapManager Instance { private set; get; }

        [SerializeField]
        private GameObject _railPrefab, _trainPrefab;

        [SerializeField]
        private Sprite _hintInbound, _hintOutbound, _hintBothways;

        private readonly List<Platform> _platforms = new();
        private readonly List<Wagon> Trains = new();

        private const int PlatformLength = 10;

        private void Awake()
        {
            Instance = this;
        }

        private Color ColorFrom255(int r, int g, int b)
        {
            return new Color(r / 255f, g / 255f, b / 255f);
        }

        private void Start()
        {
            GameStateManager.Instance.OnReset.AddListener(OnReset);

            PlacePlatform(new(-2, -10), Exit.Up, Station.Arieta, ColorFrom255(39, 108, 219)); // Dark-Blue
            PlacePlatform(new(-3, 10), Exit.Down, Station.Sorena, ColorFrom255(63, 191, 34)); // Green
            PlacePlatform(new(-15, 0), Exit.Right, Station.Esie, ColorFrom255(34, 191, 165)); // Light-Blue
            PlacePlatform(new(6, 10), Exit.Down, Station.Lai, ColorFrom255(150, 39, 219)); // Purple
        }

        public void SetupStations(IEnumerable<Station> inbounds, IEnumerable<Station> outbounds)
        {
            foreach (var p in _platforms)
            {
                var isInbound = inbounds.Contains(p.Station);
                var isOutbound = outbounds.Contains(p.Station);

                if (isInbound && isOutbound) p.AssociatedRail.ToggleSendHint(_hintBothways);
                else if (!isInbound && !isOutbound) p.AssociatedRail.ToggleSendHint(null);
                else if (isInbound) p.AssociatedRail.ToggleSendHint(_hintInbound);
                else p.AssociatedRail.ToggleSendHint(_hintOutbound);
            }
        }

        public static string StationToName(Station station)
        {
            return station switch
            {
                Station.Arieta => "Ariëta",
                Station.Sorena => "Sörena",
                Station.Esie => "Esïe",
                Station.Lai => "Laï",
                _ => station.ToString()
            };
        }

        public Platform GetStationPlatform(Station station)
            => _platforms.First(x => x.Station == station);

        public string ColorWith(Color target, string text)
        {
            return $"<color=#{Mathf.RoundToInt(target.r * 255):X2}{Mathf.RoundToInt(target.g * 255):X2}{Mathf.RoundToInt(target.b * 255):X2}>{text}</color>";
        }

        public string ColorNameWithStation(Station station, string text)
        {
            var target = _platforms.First(x => x.Station == station).Color;
            return ColorWith(target, text);
        }

        private void PlacePlatform(Vector2Int pos, Exit usedExit, Station station, Color color)
        {
            var prolongation = -Rail.GetDirection(usedExit);
            var platform = new Platform() { PositionStart = pos, PositionEnd = pos + (prolongation * PlatformLength), Exit = usedExit, Station = station, Color = color };
            for (int i = 0; i < PlatformLength; i++)
            {
                var go = Instantiate(_railPrefab, (Vector2)(pos + (prolongation * i)) * GridManager.GridWorld, Quaternion.identity);
                go.transform.rotation = Quaternion.Euler(0f, 0f, (usedExit == Exit.Left || usedExit == Exit.Right) ? 90f : 0f);
                var rail = go.GetComponent<Rail>();
                var rs = (usedExit == Exit.Left || usedExit == Exit.Right) ? SpriteManager.Instance.GetHorizontalPlatform() : SpriteManager.Instance.GetVerticalPlatform();
                rail.IsHint = false;
                rail.SR.sprite = rs.Sprite;
                rail.Exits = rs.Exits;
                rail.CanOverrides = false;
                GridManager.Instance.Register(pos + (prolongation * i), rail);

                if (i == 0)
                {
                    platform.AssociatedRail = rail;
                    _platforms.Add(platform);
                    rail.SetLabel(station, color);
                }
                else if (i == 4) rail.Platform = platform;
            }
        }

        public void OnReset()
        {
            foreach (var t in Trains)
            {
                try
                {
                    t.DestroyTrain();
                }
                catch (System.Exception e) // Just in case
                {
                    Debug.LogException(e);
                }
            }
            Trains.Clear();
        }

        public void SpawnTrain(Station from, Station to, string label, string rawLabel, TrainType train)
        {
            var platform = _platforms.First(x => x.Station == from);
            Wagon lastWagon = null;
            var prolDir = -Rail.GetDirection(platform.Exit);

            for (int i = 0; i < (train == TrainType.Commercial ? 10 : 5); i++)
            {
                var trainGo = Instantiate(_trainPrefab, (Vector2)(platform.PositionStart + (prolDir * i)) * GridManager.GridWorld, Quaternion.identity);
                var wagon = trainGo.GetComponent<Wagon>();
                wagon.TilePos = platform.PositionStart + (prolDir * i);
                wagon.Direction = platform.Exit;
                wagon.Type = train;

                if (i == 0)
                {
                    wagon.SetLabel(label, rawLabel);
                    wagon.Destination = to;
                    Trains.Add(wagon);
                }
                else
                {
                    wagon.SetRawLabel(rawLabel);
                }

                if (lastWagon != null)
                {
                    wagon.Leader = lastWagon;
                    lastWagon.Follower = wagon;
                }
                lastWagon = wagon;
            }
        }
    }

    public enum Station
    {
        Arieta,
        Sorena,
        Esie,
        Lai
    }

    public class Platform
    {
        public Vector2Int PositionStart;
        public Vector2Int PositionEnd;
        public Exit Exit;
        public Station Station;
        public Color Color;

        public Rail AssociatedRail;
    }
}
