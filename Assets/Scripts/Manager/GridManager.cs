using UnityEngine;

namespace LD59.Manager
{
    public class GridManager : MonoBehaviour
    {
        public static GridManager Instance { private set; get; }

        public static readonly int GridSize = 64;

        private void Awake()
        {
            Instance = this;
        }
    }
}
