using LD59.Map;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

        private const int PlatformLength = 5;

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

            PlacePlatform(new(-2, -10), Exit.Up, Station.Arieta, ColorFrom255(39, 108, 219)); // Blue
            PlacePlatform(new(-3, 10), Exit.Down, Station.Sorena, ColorFrom255(45, 219, 39)); // Orange
            PlacePlatform(new(-15, 0), Exit.Right, Station.Esie, ColorFrom255(105, 74, 13)); // Orange
            PlacePlatform(new(6, -10), Exit.Up, Station.Läi, ColorFrom255(150, 39, 219)); //¨Purple
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
                Station.Esie => "Ësie",
                Station.Läi => "Laï",
                _ => "Unnamed"
            };
        }

        public string ColorNameWithStation(Station station, string text)
        {
            var target = _platforms.First(x => x.Station == station).Color;
            return $"<color=#{Mathf.RoundToInt(target.r * 255):X2}{Mathf.RoundToInt(target.g * 255):X2}{Mathf.RoundToInt(target.b * 255):X2}>{text}</color>";
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
                else if (i == PlatformLength - 1) rail.Platform = platform;
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

        public void SpawnTrain(Station from, Station to, string label, string rawLabel)
        {
            var platform = _platforms.First(x => x.Station == from);
            Wagon lastWagon = null;
            var prolDir = -Rail.GetDirection(platform.Exit);

            for (int i = 0; i < PlatformLength; i++)
            {
                var trainGo = Instantiate(_trainPrefab, (Vector2)(platform.PositionStart + (prolDir * i)) * GridManager.GridWorld, Quaternion.identity);
                var wagon = trainGo.GetComponent<Wagon>();
                wagon.TilePos = platform.PositionStart + (prolDir * i);
                wagon.Direction = platform.Exit;

                if (i == 0)
                {
                    wagon.SetLabel(label, rawLabel);
                    wagon.Destination = to;
                    Trains.Add(wagon);
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
        Läi
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
