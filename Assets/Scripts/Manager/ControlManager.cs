using UnityEngine;
using UnityEngine.InputSystem;

namespace LD59.Manager
{
    public class ControlManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject _tileHint;

        private Camera _cam;

        private void Awake()
        {
            _cam = Camera.main;
        }

        private void Update()
        {
            var mousePos = Mouse.current.position.ReadValue();
            var pos = _cam.ScreenToWorldPoint(new(mousePos.x, mousePos.y, -_cam.transform.position.z)) * 100;

            var gridIndex = pos / GridManager.GridSize;
            var gridPos = new Vector2Int(Mathf.RoundToInt(gridIndex.x), Mathf.RoundToInt(gridIndex.y)) * GridManager.GridSize;

            _tileHint.transform.position = (Vector2)gridPos / 100f;
        }
    }
}
