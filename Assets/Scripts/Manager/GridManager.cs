using LD59.Map;
using System.Collections.Generic;
using UnityEngine;

namespace LD59.Manager
{
    public class GridManager : MonoBehaviour
    {
        public static GridManager Instance { private set; get; }

        public static readonly int GridSize = 64;

        private Dictionary<Vector2Int, Rail> _grid = new();

        private void Awake()
        {
            Instance = this;
        }

        public bool Has(Vector2Int pos)
            => _grid.ContainsKey(pos);
    }
}
