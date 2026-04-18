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
            _timer += Time.deltaTime * .1f;
            if (_timer > 1f)
            {
                _timer -= 1f;
                _lastPos = TilePos;
                TilePos = _nextPos;
                _nextPos = TilePos + GetDirection();
                Debug.Log("NEXT TILE");
            }

            _startPos = ((Vector2)(TilePos + _lastPos) / 2f) * GridManager.GridWorld;
            _endPos = ((Vector2)(TilePos + _nextPos) / 2f) * GridManager.GridWorld;
            Debug.Log($"{_startPos} to {_endPos} for timer {_timer}");
            transform.position = Vector2.Lerp(_startPos, _endPos, _timer);
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

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine((Vector2)_lastPos * GridManager.GridWorld, (Vector2)_nextPos * GridManager.GridWorld);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(_forward.position, _backward.position);
        }
    }
}
