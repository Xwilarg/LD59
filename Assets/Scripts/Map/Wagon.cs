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

        public Wagon Leader { set; get; }

        // Previous and next tile
        private Vector2Int _lastPos, _nextPos;
        // Pos used to calculate train rotation
        private Vector2 _startBorder, _endBorder;

        private float _timer = .5f;
        private const float TrainWheelOffset = .45f;
        private const float TrainSpeed = 2f;
        private const float SpeedIncrease = 4f;

        public float CurrTrainSpeed { private set; get; }

        private void Start()
        {
            _lastPos = TilePos - Rail.GetDirection(Direction);
            _nextPos = TilePos + Rail.GetDirection(Direction);

            CalculateBorders();
        }

        private void CalculateBorders()
        {
            _startBorder = (Vector2)(_lastPos + TilePos) / 2f * GridManager.GridWorld;
            _endBorder = (Vector2)(_nextPos + TilePos) / 2f * GridManager.GridWorld;
        }

        private void Update()
        {
            if (Leader == null)
            {

                var speedIncr = 1f;
                
                if (GridManager.Instance.Has(TilePos))
                {
                    var rail = GridManager.Instance.Get(TilePos);
                    if (rail.Signal != null && !rail.Signal.IsGreen) speedIncr = -1f; // Red light in front of us!
                }
                
                CurrTrainSpeed = Mathf.Clamp(CurrTrainSpeed + Time.deltaTime * SpeedIncrease * speedIncr, 0f, TrainSpeed);
            }
            else // Front wagon lead others behind
            {
                CurrTrainSpeed = Leader.CurrTrainSpeed;
            }

            _timer += Time.deltaTime * CurrTrainSpeed;
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
                        if (tile.Platform != null)
                        {
                            Destroy(gameObject);
                            return;
                        }

                        Direction = tile.GetExit(Revert());

                        if (Direction == Exit.None)
                        {
                            Crash("Train reached missconfigured junction");
                        }
                        else
                        {
                            // Debug.Log($"Next exit it toward {Direction}");
                            _nextPos = TilePos + Rail.GetDirection(Direction);
                            CalculateBorders();

                            if (Leader != null && !Leader.IsPosOnTrack(_nextPos))
                            {
                                Crash("Wagon got disconnected from its leading one");
                            }
                        }
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

            transform.position = Vector2.Lerp(_startBorder, _endBorder, _timer);

            /*
            var t1 = _timer - TrainWheelOffset;
            var t2 = _timer + TrainWheelOffset;

            var p1 = t1 < .5f ? Vector2.LerpUnclamped(_startBorder, (Vector2)TilePos * GridManager.GridWorld, t1 * 2f) : Vector2.LerpUnclamped((Vector2)TilePos * GridManager.GridWorld, _endBorder, t1 * 2f - 1f);
            var p2 = t2 < .5f ? Vector2.LerpUnclamped(_startBorder, (Vector2)TilePos * GridManager.GridWorld, t2 * 2f) : Vector2.LerpUnclamped((Vector2)TilePos * GridManager.GridWorld, _endBorder, t2 * 2f - 1f);

            var rot = Mathf.Atan2(p2.y - p1.y, p2.x - p1.x) * Mathf.Rad2Deg + 90f;
            */
            var rot = Mathf.Atan2(_endBorder.y - _startBorder.y, _endBorder.x - _startBorder.x) * Mathf.Rad2Deg + 90f;
            transform.rotation = Quaternion.Euler(0f, 0f, rot);
        }

        public bool IsPosOnTrack(Vector2Int pos)
        {
            return pos == _lastPos || pos == TilePos;
        }

        public void Crash(string reason)
        {
            GameStateManager.Instance.Loose(reason);
            Destroy(gameObject);
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
            Gizmos.DrawLine(_startBorder, _endBorder);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(_forward.position, _backward.position);
        }
    }
}
