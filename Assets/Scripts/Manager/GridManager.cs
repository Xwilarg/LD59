using LD59.Map;
using System.Collections.Generic;
using UnityEngine;

namespace LD59.Manager
{
    public class GridManager : MonoBehaviour
    {
        public static GridManager Instance { private set; get; }

        public static readonly int GridSize = 64;
        public static readonly float GridWorld = .64f;
        public static readonly float HalfGridWorld = .32f;

        private Dictionary<Vector2Int, Rail> _grid = new();

        private void Awake()
        {
            Instance = this;
        }

        public void Register(Vector2Int pos, Rail rail)
            => _grid.Add(pos, rail);

        public void Delete(Vector2Int pos)
            => _grid.Remove(pos);

        public bool Has(Vector2Int pos)
            => _grid.ContainsKey(pos);

        public Rail Get(Vector2Int pos)
            => _grid[pos];
    }
}
