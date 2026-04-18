using LD59.Map;
using System.Collections.Generic;
using UnityEngine;

namespace LD59.Manager
{
    public class MapManager : MonoBehaviour
    {
        public static MapManager Instance { private set; get; }

        [SerializeField]
        private GameObject _railPrefab, _trainPrefab;

        private readonly List<Platform> _platforms = new();

        private const int PlatformLength = 5;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            PlacePlatform(new(0, -7), Exit.Up, Vector2Int.down);
            PlacePlatform(new(2, 7), Exit.Down, Vector2Int.up);
        }

        private void PlacePlatform(Vector2Int pos, Exit usedExit, Vector2Int prolongation)
        {
            var platform = new Platform() { PositionStart = pos, PositionEnd = pos + (prolongation * PlatformLength), Prolongation = prolongation, Exit = usedExit };
            for (int i = 0; i < PlatformLength; i++)
            {
                var go = Instantiate(_railPrefab, (Vector2)(pos + (prolongation * i)) * GridManager.GridWorld, Quaternion.identity);
                var rail = go.GetComponent<Rail>();
                var rs = SpriteManager.Instance.GetPlatform();
                rail.IsHint = false;
                rail.SR.sprite = rs.Sprite;
                rail.Exits = rs.Exits;
                rail.CanOverrides = false;
                GridManager.Instance.Register(pos + (prolongation * i), rail);

                if (i == 0) _platforms.Add(platform);
                else if (i == PlatformLength - 1) rail.Platform = platform;
            }
        }

        public void SpawnTrain()
        {
            var platform = _platforms[Random.Range(0, _platforms.Count)];

            for (int i = 0; i < PlatformLength; i++)
            {
                var trainGo = Instantiate(_trainPrefab, (Vector2)(platform.PositionStart + (platform.Prolongation * i)) * GridManager.GridWorld, Quaternion.identity);
                var wagon = trainGo.GetComponent<Wagon>();
                wagon.TilePos = platform.PositionStart + (platform.Prolongation * i);
                wagon.Direction = platform.Exit;
            }
        }
    }

    public class Platform
    {
        public Vector2Int PositionStart;
        public Vector2Int PositionEnd;
        public Vector2Int Prolongation;
        public Exit Exit;
    }
}
