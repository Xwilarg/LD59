using LD59.Manager;
using UnityEngine;

namespace LD59.Map
{
    public class Wagon : MonoBehaviour
    {
        [SerializeField]
        public Transform _forward, _backward;

        public Vector2Int TilePos { set; get; }
        public Exit Direction { set; get; }

        private Vector2Int _lastPos;
        private Vector2Int _nextPos;

        private Vector2 _startPos, _endPos;

        private float _timer = .5f;

        private void Start()
        {
            _lastPos = TilePos - GetDirection();
            _nextPos = TilePos + GetDirection();
        }

        private void Update()
        {
            _timer += Time.deltaTime * 1f;
            if (_timer > 1f)
            {
                _timer -= 1f;
                _lastPos = TilePos;
                TilePos = _nextPos;
                // Debug.Log($"Railed passed, current at {TilePos} going to {_nextPos} (opposite exit is {Revert()})");

                if (GridManager.Instance.Has(TilePos))
                {
                    var tile = GridManager.Instance.Get(TilePos);
                    // Debug.Log($"Tile at {TilePos} have the following exists: {tile.Exits}");
                    if (tile.Exits.HasFlag(Revert()))
                    {
                        Direction = tile.GetExit(Revert());
                        // Debug.Log($"Next exit it toward {Direction}");
                        _nextPos = TilePos + GetDirection();
                    }
                    else
                    {
                        Crash("Train reached missplaced track");
                    }
                }
                else
                {
                    Crash("Train reached the end of the line");
                }
            }

            _startPos = ((Vector2)(TilePos + _lastPos) / 2f) * GridManager.GridWorld;
            _endPos = ((Vector2)(TilePos + _nextPos) / 2f) * GridManager.GridWorld;
            transform.position = Vector2.Lerp(_startPos, _endPos, _timer);

            var rot = Mathf.Atan2(_endPos.y - _startPos.y, _endPos.x - _startPos.x) * Mathf.Rad2Deg + 90f;
            transform.rotation = Quaternion.Euler(0f, 0f, rot);
        }

        public void Crash(string reason)
        {
            Debug.LogWarning($"Wragon crashed: {reason}");
            Destroy(gameObject);
        }

        public Vector2Int GetDirection()
        {
            return Direction switch
            {
                Exit.Up => Vector2Int.up,
                Exit.Down => Vector2Int.down,
                Exit.Left => Vector2Int.left,
                Exit.Right => Vector2Int.right,
                _ => throw new System.NotImplementedException($"Invalid exit {Direction}")
            };
        }

        public Exit Revert()
        {
            return Direction switch
            {
                Exit.Up => Exit.Down,
                Exit.Down => Exit.Up,
                Exit.Left => Exit.Right,
                Exit.Right => Exit.Left,
                _ => throw new System.NotImplementedException($"Invalid exit {Direction}")
            };
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine((Vector2)_lastPos * GridManager.GridWorld, (Vector2)_nextPos * GridManager.GridWorld);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(_forward.position, _backward.position);
        }
    }
}
