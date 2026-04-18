using LD59.Map;
using System.Collections.Generic;
using UnityEngine;

namespace LD59.Manager
{
    public class MapManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject _railPrefab;

        private readonly List<Platform> _platforms = new();

        private void Start()
        {
            PlacePlatform(new(0, -7), Exit.Up);
        }

        private void PlacePlatform(Vector2Int pos, Exit usedExit)
        {
            var go = Instantiate(_railPrefab, (Vector2)pos * (GridManager.GridSize / 100f), Quaternion.identity);
            var rail = go.GetComponent<Rail>();
            var platform = SpriteManager.Instance.GetPlatform();
            rail.SR.sprite = platform.Sprite;
            rail.Exits = platform.Exits;
            rail.CanOverrides = false;
            GridManager.Instance.Register(pos, rail);

            _platforms.Add(new() { Position = pos, Exit = usedExit });
        }
    }

    public class Platform
    {
        public Vector2Int Position;
        public Exit Exit;
    }
}
