using UnityEngine;
using UnityEngine.InputSystem;

namespace LD59.Manager
{
    public class ControlManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject _tileHint;
        private SpriteRenderer _tileSr;

        private int _currIndex;

        private Camera _cam;

        private Vector2Int _gridIndex;

        private RotatedSprite[] _sprites;

        private void Awake()
        {
            _cam = Camera.main;

            _tileSr = _tileHint.GetComponent<SpriteRenderer>();
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
            _gridIndex = new Vector2Int(Mathf.RoundToInt(index.x), Mathf.RoundToInt(index.y));
            var gridPos = _gridIndex * GridManager.GridSize;

            _tileHint.transform.position = (Vector2)gridPos / 100f;
        }

        public void OnPlace(InputAction.CallbackContext value)
        {
            Instantiate(_tileHint);
        }

        public void OnRotate(InputAction.CallbackContext value)
        {
            if (value.phase == InputActionPhase.Started)
            {
                _currIndex = (_currIndex + 1) % _sprites.Length;
                var tile = _sprites[_currIndex];
                _tileHint.transform.rotation = Quaternion.Euler(0f, 0f, tile.Rotation);
                _tileSr.sprite = tile.Sprite;
            }
        }
    }
}
