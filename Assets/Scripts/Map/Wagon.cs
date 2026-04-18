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

        private float _timer = .5f;

        private void Awake()
        {
            _nextPos = GetNextPosition();
        }

        private void Update()
        {
            _timer += Time.deltaTime * .5f;
            if (_timer > 1f)
            {
                _timer -= 1f;
                TilePos = _nextPos;
                _nextPos = GetNextPosition();
            }

            //transform.position = Mathf.Lerp
        }

        public Vector2Int GetNextPosition()
        {
            return TilePos + Direction switch
            {
                Exit.Up => Vector2Int.up,
                Exit.Down => Vector2Int.down,
                Exit.Left => Vector2Int.left,
                Exit.Right => Vector2Int.right,
                _ => throw new System.NotImplementedException()
            };
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(_forward.position, _backward.position);
        }
    }
}
