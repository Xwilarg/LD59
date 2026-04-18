using LD59.Map;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LD59.Manager
{
    public class ControlManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject _tileHint;
        private SpriteRenderer _tileSr;
        private Rail _tileRail;

        private int _currIndex;

        private Camera _cam;

        private Vector2Int _gridIndex;

        private RotatedSprite[] _sprites;

        private void Awake()
        {
            _cam = Camera.main;

            _tileSr = _tileHint.GetComponent<SpriteRenderer>();
            _tileRail = _tileHint.GetComponent<Rail>();
        }

        private void Start()
        {
            _sprites = new[]
            {
                SpriteManager.Instance.GetVertical(),
                SpriteManager.Instance.GetTurnDownRight(),
                SpriteManager.Instance.GetHorizontal(),
                SpriteManager.Instance.GetTurnDownLeft(),
                SpriteManager.Instance.GetVertical(),
                SpriteManager.Instance.GetTurnUpLeft(),
                SpriteManager.Instance.GetHorizontal(),
                SpriteManager.Instance.GetTurnUpRight()
            };
        }

        private void Update()
        {
            var mousePos = Mouse.current.position.ReadValue();
            var pos = _cam.ScreenToWorldPoint(new(mousePos.x, mousePos.y, -_cam.transform.position.z)) * 100;

            var index = pos / GridManager.GridSize;
            var newIndex = new Vector2Int(Mathf.RoundToInt(index.x), Mathf.RoundToInt(index.y));

            if (_gridIndex != newIndex)
            {
                _gridIndex = newIndex;
                var gridPos = newIndex * GridManager.GridSize;
                _tileHint.transform.position = (Vector2)gridPos / 100f;

                UpdateSprite();
            }
        }

        private void UpdateSprite()
        {
            var tile = _sprites[_currIndex];

            if (GridManager.Instance.Has(_gridIndex))
            {
                var tExisting = GridManager.Instance.Get(_gridIndex);
                if (!tExisting.CanOverrides)
                {
                    _tileSr.color = new Color(1f, 0f, 0f, _tileSr.color.a);
                    return;
                }
                var existing = tExisting.Exits;
                var exits = tile.Exits | existing;

                Debug.Log($"[PLAC] Hinting tile over an existing one, mixing {tile.Exits} and {existing} which gives {exits}");
                var matchingTile = SpriteManager.Instance.All.First(x => x.Exits == exits);

                tile = matchingTile;
            }

            _tileHint.transform.rotation = Quaternion.Euler(0f, 0f, tile.Rotation);
            _tileSr.sprite = tile.Sprite;
            _tileSr.color = new Color(1f, 1f, 1f, _tileSr.color.a);
            _tileRail.Exits = tile.Exits;
        }

        public void OnPlace(InputAction.CallbackContext value)
        {
            if (value.phase == InputActionPhase.Started)
            {
                if (GridManager.Instance.Has(_gridIndex))
                {
                    var elem = GridManager.Instance.Get(_gridIndex);
                    elem.Exits = _tileRail.Exits;
                    elem.transform.rotation = Quaternion.Euler(0f, 0f, _tileRail.transform.rotation.eulerAngles.z);
                    elem.SR.sprite = _tileRail.SR.sprite;
                    Debug.Log($"[PLAC] Placed tile with exists {elem.Exits}");
                }
                else
                {
                    var go = Instantiate(_tileHint);
                    var rail = go.GetComponent<Rail>();
                    GridManager.Instance.Register(_gridIndex, rail);
                    rail.SR.sortingLayerName = "Default";
                    rail.SR.color = Color.white;
                    rail.Exits = _tileRail.Exits;
                    Debug.Log($"[PLAC] Placed tile with exists {rail.Exits}");
                }

                UpdateSprite();
            }
        }

        public void OnRotate(InputAction.CallbackContext value)
        {
            if (value.phase == InputActionPhase.Started)
            {
                _currIndex = (_currIndex + 1) % _sprites.Length;
                UpdateSprite();
            }
        }
    }
}
