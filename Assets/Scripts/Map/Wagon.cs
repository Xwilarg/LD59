using LD59.Manager;
using LD59.SO;
using TMPro;
using UnityEngine;

namespace LD59.Map
{
    public class Wagon : MonoBehaviour
    {
        [SerializeField]
        private Transform _forward, _backward;

        [SerializeField]
        private TMP_Text _label;
        private string _trainLabel;
        public string RawTrainLabel { private set; get; }

        [SerializeField]
        private AudioSource _trainNoise;

        public Vector2Int TilePos { set; get; }
        public Exit Direction { set; get; }
        public Station Destination { set; get; } = (Station)(-1);

        public TrainType Type { set; get; }

        public Wagon Leader { set; get; }
        public Wagon Follower { set; get; }

        // Previous and next tile
        private Vector2Int _lastPos, _nextPos;
        // Pos used to calculate train rotation
        private Vector2 _startBorder, _endBorder;

        private float _timer = .5f;
        private const float TrainWheelOffset = .45f;
        private float TrainSpeed = 2f;
        private const float SpeedIncrease = 4f;

        private bool _isUnresponding;
        private Vector2 _lastVelocity;

        private BoxCollider2D _coll;
        private Rigidbody2D _rb;
        private SpriteRenderer _sr;

        public float CurrTrainSpeed { private set; get; }

        private void Awake()
        {
            _label.gameObject.SetActive(false);
            _coll = GetComponent<BoxCollider2D>();
            _rb = GetComponent<Rigidbody2D>();
            _sr = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            _lastPos = TilePos - Rail.GetDirection(Direction);
            _nextPos = TilePos + Rail.GetDirection(Direction);

            CalculateBorders();

            _sr.sprite = SpriteManager.Instance.GetWagonSprite(Type);
            TrainSpeed = Type switch
            {
                TrainType.Normal => 2f,
                TrainType.HighSpeed => 4f,
                TrainType.Commercial => .75f,
                _ => throw new System.NotImplementedException()
            };

            if (Leader == null) _trainNoise.Play();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.TryGetComponent<Wagon>(out var wagon))
            {
                if (wagon != Follower && wagon != Leader)
                {
                    Crash($"{RawTrainLabel} crashed into {wagon.RawTrainLabel}");
                }
            }
        }

        public void SetLabel(string text, string rawText)
        {
            RawTrainLabel = rawText;
            _trainLabel = text;
            _label.gameObject.SetActive(true);
            _label.text = text;
        }

        public void SetRawLabel(string rawText)
        {
            RawTrainLabel = rawText;
        }

        private void CalculateBorders()
        {
            _startBorder = (Vector2)(_lastPos + TilePos) / 2f * GridManager.GridWorld;
            _endBorder = (Vector2)(_nextPos + TilePos) / 2f * GridManager.GridWorld;
        }

        private void Update()
        {
            if (_isUnresponding)
            {
                _trainNoise.volume = 0f;
                //transform.position += (Vector3)_lastVelocity.normalized * Time.deltaTime; // TODO: Placeholder with deltatime since previous code wasn't working
                return;
            }

            if (Leader == null)
            {
                var speedIncr = 1f;
                
                if (GridManager.Instance.Has(TilePos))
                {
                    var rail = GridManager.Instance.Get(TilePos);
                    if (rail.Signal != null && !rail.Signal.IsGreen) speedIncr = -1f; // Red light in front of us!
                }
                
                CurrTrainSpeed = Mathf.Clamp(CurrTrainSpeed + Time.deltaTime * SpeedIncrease * speedIncr, 0f, TrainSpeed);
                _trainNoise.volume = CurrTrainSpeed / TrainSpeed;
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
                        if (tile.Platform != null && Leader == null) // Only leading wagon can do playform check
                        {
                            if (tile.Platform.Station != Destination)
                            {
                                Crash($"Train reached {MapManager.StationToName(tile.Platform.Station)} instead of {MapManager.StationToName(Destination)}");
                                return;
                            }
                            else
                            {
                                StoryManager.Instance.ArriveTrain(_trainLabel);
                                DestroyTrain();
                            }
                            return;
                        }

                        Direction = tile.GetExit(Revert());

                        if (Direction == Exit.None)
                        {
                            Crash("Train reached missconfigured junction");
                            return;
                        }
                        else
                        {
                            // Debug.Log($"Next exit it toward {Direction}");
                            _nextPos = TilePos + Rail.GetDirection(Direction);
                            CalculateBorders();

                            if (Leader != null && !Leader.IsPosOnTrack(_nextPos))
                            {
                                Crash("Wagon got disconnected from its leading one");
                                return;
                            }
                            else
                            {
                                _lastVelocity = ((Vector2)(_nextPos - TilePos)).normalized * CurrTrainSpeed;
                            }
                        }
                    }
                    else
                    {
                        Crash("Train reached missplaced track");
                        return;
                    }
                }
                else
                {
                    Crash("Train reached the end of the line");
                    return;
                }
            }

            transform.position = Vector2.Lerp(_startBorder, _endBorder, _timer);
            _label.transform.position = Vector2.LerpUnclamped(_startBorder, _endBorder, _timer + .85f);

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
            Debug.LogWarning(reason);
            GameStateManager.Instance.Loose(reason);
            _isUnresponding = true;

            //_coll.isTrigger = false;
            //_rb.bodyType = RigidbodyType2D.Dynamic;
            CurrTrainSpeed = 0f;
        }

        public void DestroyTrain()
        {
            if (Follower != null) Follower.DestroyTrain();

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
