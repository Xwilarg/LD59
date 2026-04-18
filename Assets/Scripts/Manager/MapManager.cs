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

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            PlacePlatform(new(0, -7), Exit.Up);
        }

        private void PlacePlatform(Vector2Int pos, Exit usedExit)
        {
            var go = Instantiate(_railPrefab, (Vector2)pos * GridManager.GridWorld, Quaternion.identity);
            var rail = go.GetComponent<Rail>();
            var platform = SpriteManager.Instance.GetPlatform();
            rail.SR.sprite = platform.Sprite;
            rail.Exits = platform.Exits;
            rail.CanOverrides = false;
            GridManager.Instance.Register(pos, rail);

            _platforms.Add(new() { Position = pos, Exit = usedExit });
        }

        public void SpawnTrain()
        {
            var platform = _platforms[Random.Range(0, _platforms.Count)];

            var trainGo = Instantiate(_trainPrefab, (Vector2)platform.Position * GridManager.GridWorld, Quaternion.identity);
            var wagon = trainGo.GetComponent<Wagon>();
            wagon.TilePos = platform.Position;
            wagon.Direction = platform.Exit;
        }
    }

    public class Platform
    {
        public Vector2Int Position;
        public Exit Exit;
    }
}
