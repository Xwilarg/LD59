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

        private readonly List<Platform> _platforms = new();
        private readonly List<Wagon> Trains = new();

        private const int PlatformLength = 5;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            GameStateManager.Instance.OnReset.AddListener(OnReset);

            PlacePlatform(new(0, -10), Exit.Up, Vector2Int.down, Station.Arieta);
            PlacePlatform(new(2, 10), Exit.Down, Vector2Int.up, Station.Sorena);
            PlacePlatform(new(-15, 0), Exit.Right, Vector2Int.left, Station.Esie);
        }

        public static string StationToName(Station station)
        {
            return station switch
            {
                Station.Arieta => "Ariëta",
                Station.Sorena => "Sörena",
                Station.Esie => "Ësie",
                _ => "Unnamed"
            };
        }

        private void PlacePlatform(Vector2Int pos, Exit usedExit, Vector2Int prolongation, Station station)
        {
            var platform = new Platform() { PositionStart = pos, PositionEnd = pos + (prolongation * PlatformLength), Prolongation = prolongation, Exit = usedExit, Station = station };
            for (int i = 0; i < PlatformLength; i++)
            {
                var go = Instantiate(_railPrefab, (Vector2)(pos + (prolongation * i)) * GridManager.GridWorld, Quaternion.identity);
                go.transform.rotation = Quaternion.Euler(0f, 0f, (usedExit == Exit.Left || usedExit == Exit.Right) ? 90f : 0f);
                var rail = go.GetComponent<Rail>();
                var rs = SpriteManager.Instance.GetPlatform();
                rail.IsHint = false;
                rail.SR.sprite = rs.Sprite;
                rail.Exits = rs.Exits;
                rail.CanOverrides = false;
                GridManager.Instance.Register(pos + (prolongation * i), rail);

                if (i == 0)
                {
                    _platforms.Add(platform);
                    rail.SetLabel(station);
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

        public void SpawnTrain(Station from, Station to, string label)
        {
            var platform = _platforms.First(x => x.Station == from);
            Wagon lastWagon = null;

            for (int i = 0; i < PlatformLength; i++)
            {
                var trainGo = Instantiate(_trainPrefab, (Vector2)(platform.PositionStart + (platform.Prolongation * i)) * GridManager.GridWorld, Quaternion.identity);
                var wagon = trainGo.GetComponent<Wagon>();
                wagon.TilePos = platform.PositionStart + (platform.Prolongation * i);
                wagon.Direction = platform.Exit;

                if (i == 0)
                {
                    wagon.SetLabel(label);
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
        Esie
    }

    public class Platform
    {
        public Vector2Int PositionStart;
        public Vector2Int PositionEnd;
        public Vector2Int Prolongation;
        public Exit Exit;
        public Station Station;
    }
}
