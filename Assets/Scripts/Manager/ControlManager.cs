using LD59.Map;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace LD59.Manager
{
    public class ControlManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject _tileHint;
        private SpriteRenderer _tileSr;
        private Rail _tileRail;

        [SerializeField]
        private GameObject _deleteHighlight, _configureHighlight;

        [SerializeField]
        private GameObject _signalHint;

        private Transform[] _hints;

        [SerializeField]
        private TMP_Text _toolDescription;

        [SerializeField]
        private GameObject _toolbar;
        private Button[] _toolButtons;

        private int _currIndex;

        private Camera _cam;

        private Vector2Int _gridIndex;

        private RotatedSprite[] _sprites;

        private Tool _currentTool;

        private void Awake()
        {
            _cam = Camera.main;
            _hints = new[] { _tileHint.transform, _deleteHighlight.transform, _signalHint.transform, _configureHighlight.transform };

            _tileSr = _tileHint.GetComponent<SpriteRenderer>();
            _tileRail = _tileHint.GetComponent<Rail>();

            _toolButtons = _toolbar.GetComponentsInChildren<Button>();
            _toolButtons[0].onClick.Invoke();
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

                foreach (var t in _hints) t.position = (Vector2)gridPos / 100f;

                UpdateSprite();
            }
        }

        private void UpdateSprite()
        {
            var tile = _sprites[_currIndex];
            bool allowOverrides = true;

            if (GridManager.Instance.Has(_gridIndex))
            {
                var tExisting = GridManager.Instance.Get(_gridIndex);
                allowOverrides = tExisting.CanOverrides;
                var existing = tExisting.Exits;
                var exits = tile.Exits | existing;

                //Debug.Log($"[PLAC] Hinting tile over an existing one, mixing {tile.Exits} and {existing} which gives {exits}");
                var matchingTile = SpriteManager.Instance.All.First(x => x.Exits == exits);

                tile = matchingTile;
            }

            _tileHint.transform.rotation = Quaternion.Euler(0f, 0f, tile.Rotation);
            _tileSr.sprite = tile.Sprite;
            _tileSr.color = allowOverrides ? new Color(1f, 1f, 1f, _tileSr.color.a) : new Color(1f, 0f, 0f, _tileSr.color.a); ;
            _tileRail.Exits = tile.Exits;
        }

        private bool IsClickOnUI()
        {
            PointerEventData pointerEventData = new(EventSystem.current)
            {
                position = Mouse.current.position.ReadValue()
            };
            List<RaycastResult> raycastResultsList = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerEventData, raycastResultsList);
            for (int i = 0; i < raycastResultsList.Count; i++)
            {
                if (raycastResultsList[i].gameObject.TryGetComponent<Button>(out var _))
                {
                    return true;
                }
            }
            return false;
        }

        public void OnPlace(InputAction.CallbackContext value)
        {
            if (value.phase == InputActionPhase.Started && !IsClickOnUI())
            {
                if (_currentTool == Tool.Rail)
                {
                    if (GridManager.Instance.Has(_gridIndex))
                    {
                        var elem = GridManager.Instance.Get(_gridIndex);
                        if (!elem.CanOverrides)
                        {
                            WarningManager.Instance.ShowWarning("This tile can't be modified");
                            return;
                        }
                        elem.Exits = _tileRail.Exits;
                        elem.transform.rotation = Quaternion.Euler(0f, 0f, _tileRail.transform.rotation.eulerAngles.z);
                        elem.SR.sprite = _tileRail.SR.sprite;
                        //Debug.Log($"[PLAC] Placed tile with exists {elem.Exits}");
                    }
                    else
                    {
                        var go = Instantiate(_tileHint);
                        var rail = go.GetComponent<Rail>();
                        GridManager.Instance.Register(_gridIndex, rail);
                        rail.IsHint = false;
                        rail.SR.sortingLayerName = "Default";
                        rail.SR.color = Color.white;
                        rail.Exits = _tileRail.Exits;
                        //Debug.Log($"[PLAC] Placed tile with exists {rail.Exits}");
                    }
                }
                else if (_currentTool == Tool.Eraser)
                {
                    if (GridManager.Instance.Has(_gridIndex))
                    {
                        var tile = GridManager.Instance.Get(_gridIndex);

                        if (tile.CanOverrides)
                        {
                            if (tile.Signal != null)
                            {
                                Destroy(tile.Signal.gameObject);
                            }
                            Destroy(tile.gameObject);
                            GridManager.Instance.Delete(_gridIndex);
                        }
                        else
                        {
                            WarningManager.Instance.ShowWarning("This tile can't be deleted");
                        }
                    }
                }
                else if (_currentTool == Tool.Signal)
                {
                    if (GridManager.Instance.Has(_gridIndex))
                    {
                        var tile = GridManager.Instance.Get(_gridIndex);

                        if (tile.CanOverrides)
                        {
                            if (tile.HaveManyExits())
                            {
                                WarningManager.Instance.ShowWarning("Signals can't be placed on a junction");
                            }
                            else
                            {
                                if (tile.Signal == null)
                                {
                                    var signalGo = Instantiate(_signalHint);
                                    var signal = signalGo.GetComponent<Signal>();
                                    signal.SR.color = Color.white;
                                    signal.SR.sortingLayerName = "Signal";
                                    tile.Signal = signal;
                                }
                            }
                        }
                        else
                        {
                            WarningManager.Instance.ShowWarning("This tile can't be modified");
                        }
                    }
                    else
                    {
                        WarningManager.Instance.ShowWarning("A track must be placed before adding a signal");
                    }
                }
                else if (_currentTool == Tool.Configure)
                {
                    if (GridManager.Instance.Has(_gridIndex))
                    {
                        var tile = GridManager.Instance.Get(_gridIndex);

                        if (tile.Signal != null)
                        {
                            tile.Signal.IsGreen = !tile.Signal.IsGreen;
                            tile.Signal.SR.sprite = tile.Signal.IsGreen ? SpriteManager.Instance.LightOn : SpriteManager.Instance.LightOff;
                        }
                    }
                }
            }
        }

        public void OnRotate(InputAction.CallbackContext value)
        {
            if (value.phase == InputActionPhase.Started && !IsClickOnUI())
            {
                if (_currentTool == Tool.Rail)
                {
                    _currIndex = (_currIndex + 1) % _sprites.Length;
                    UpdateSprite();
                }
                else if (_currentTool == Tool.Eraser)
                {
                    if (GridManager.Instance.Has(_gridIndex))
                    {
                        var tile = GridManager.Instance.Get(_gridIndex);

                        if (tile.CanOverrides && tile.Signal != null)
                        {
                            Destroy(tile.Signal.gameObject);
                            tile.Signal = null;
                        }
                    }
                }
                else if (_currentTool == Tool.Signal)
                {
                    // Nothing to do
                }
                else if (_currentTool == Tool.Configure)
                {
                    if (GridManager.Instance.Has(_gridIndex))
                    {
                        var tile = GridManager.Instance.Get(_gridIndex);

                        if (tile.HaveManyExits())
                        {
                            tile.UpdatePathIndex();
                        }
                    }
                }
            }
        }

        public void SelectRailTool()
        {
            _toolDescription.text = "<b>Rails tool<b>\nLeft click: Place rail\nRight click: Rotate rail";
            foreach (var t in _hints) t.gameObject.SetActive(false);
            _tileHint.gameObject.SetActive(true);

            _currentTool = Tool.Rail;
        }

        public void SelectErasedTool()
        {
            _toolDescription.text = "<b>Eraser tool<b>\nLeft click: Delete rail\nRight click: Delete signal";
            foreach (var t in _hints) t.gameObject.SetActive(false);
            _deleteHighlight.SetActive(true);

            _currentTool = Tool.Eraser;
        }

        public void SelectSignalTool()
        {
            _toolDescription.text = "<b>Signal tool<b>\nLeft click: Place signal";
            foreach (var t in _hints) t.gameObject.SetActive(false);
            _signalHint.SetActive(true);

            _currentTool = Tool.Signal;
        }

        public void SelectConfigureTool()
        {
            _toolDescription.text = "<b>Configure tool<b>\nLeft click: Switch signal\nRight click: Switch junction";
            foreach (var t in _hints) t.gameObject.SetActive(false);
            _configureHighlight.SetActive(true);

            _currentTool = Tool.Configure;
        }

        public void OnPress1(InputAction.CallbackContext value) { if (value.phase == InputActionPhase.Started && _toolButtons[0].interactable) _toolButtons[0].onClick.Invoke(); }
        public void OnPress2(InputAction.CallbackContext value) { if (value.phase == InputActionPhase.Started && _toolButtons[1].interactable) _toolButtons[1].onClick.Invoke(); }
        public void OnPress3(InputAction.CallbackContext value) { if (value.phase == InputActionPhase.Started && _toolButtons[2].interactable) _toolButtons[2].onClick.Invoke(); }
        public void OnPress4(InputAction.CallbackContext value) { if (value.phase == InputActionPhase.Started && _toolButtons[3].interactable) _toolButtons[3].onClick.Invoke(); }
    }

    public enum Tool
    {
        Rail,
        Signal,
        Eraser,
        Configure
    }
}
