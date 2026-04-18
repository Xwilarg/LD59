using LD59.Manager;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace LD59.Map
{
    public class Rail : MonoBehaviour
    {
        [SerializeField]
        private LineRenderer _lineHint;

        private readonly List<ExitPair> _allPaths = new();
        private int _pathIndex;

        private Exit _exits;
        public Exit Exits
        {
            set
            {
                _exits = value;
                UpdateRender();
            }
            get => _exits;
        }

        public bool IsHint { set; get; } = true;

        public bool CanOverrides { set; get; } = true;
        public Platform Platform { set; get; }

        public Signal Signal { set; get; }

        public SpriteRenderer SR { private set; get; }

        private void Awake()
        {
            SR = GetComponent<SpriteRenderer>();
        }

        public void UpdatePathIndex()
        {
            _pathIndex = (_pathIndex + 1) % _allPaths.Count;
            UpdateRender();
        }

        public void UpdateRender()
        {
            if (IsHint) return; // Hint tiles aren't in the world yet so we don't show any additional info on them

            if (HaveManyExits())
            {
                if (Signal != null)
                {
                    Destroy(Signal.gameObject);
                    Signal = null;
                }
                _lineHint.gameObject.SetActive(true);

                _lineHint.positionCount = 2;
                ExitPair pair = _allPaths[_pathIndex];

                var pos1 = (Vector3)((Vector2)transform.position * 2f + ((Vector2)GetDirection(pair.E1) * GridManager.GridWorld)) / 2f;
                var pos2 = (Vector3)((Vector2)transform.position * 2f + ((Vector2)GetDirection(pair.E2) * GridManager.GridWorld)) / 2f;
                _lineHint.SetPositions(new[] { pos1, pos2 });

                // Update list of exits
                var allExits = ExitFlagToList(Exits);
                for (int a = 0; a < allExits.Length; a++)
                {
                    for (int b = a + 1; b < allExits.Length; b++)
                    {
                        var e1 = allExits[a];
                        var e2 = allExits[b];
                        Assert.AreNotEqual(e1, e2, $"Trying to make a path where entrance lead to exit");

                        if (!_allPaths.Any(x => (x.E1 == e1 && x.E2 == e2) || (x.E1 == e2 && x.E2 == e1)))
                        {
                            _allPaths.Add(new() { E1 = e1, E2 = e2 });
                        }
                    }
                }
            }
            else
            {
                _lineHint.gameObject.SetActive(false);
                if (_allPaths.Count == 0)
                {
                    var list = ExitFlagToList(Exits);
                    Assert.IsTrue(list.Length == 2, $"Rail path doesn't isn't a line (list values: {string.Join(", ", list)})");
                    _allPaths.Add(new ExitPair() { E1 = list[0], E2 = list[1] });
                }
            }
        }

        public bool HaveManyExits()
        {
            int count = 0;
            if (Exits.HasFlag(Exit.Up)) count++;
            if (Exits.HasFlag(Exit.Down)) count++;
            if (Exits.HasFlag(Exit.Left)) count++;
            if (Exits.HasFlag(Exit.Right)) count++;

            return count > 2;
        }

        private static Exit[] ExitFlagToList(Exit exits)
        {
            List<Exit> list = new();
            if (exits.HasFlag(Exit.Up)) list.Add(Exit.Up);
            if (exits.HasFlag(Exit.Down)) list.Add(Exit.Down);
            if (exits.HasFlag(Exit.Left)) list.Add(Exit.Left);
            if (exits.HasFlag(Exit.Right)) list.Add(Exit.Right);
            return list.ToArray();
        }

        public Exit GetExit(Exit source)
        {
            var path = _allPaths[_pathIndex];
            var possibles = path.E1 | path.E2;

            if (!possibles.HasFlag(source)) return Exit.None; // Track not accessible

            if (source != Exit.Up && possibles.HasFlag(Exit.Up)) return Exit.Up;
            if (source != Exit.Down && possibles.HasFlag(Exit.Down)) return Exit.Down;
            if (source != Exit.Left && possibles.HasFlag(Exit.Left)) return Exit.Left;
            if (source != Exit.Right && possibles.HasFlag(Exit.Right)) return Exit.Right;
            return Exit.None;
        }

        public static Vector2Int GetDirection(Exit direction)
        {
            return direction switch
            {
                Exit.Up => Vector2Int.up,
                Exit.Down => Vector2Int.down,
                Exit.Left => Vector2Int.left,
                Exit.Right => Vector2Int.right,
                _ => throw new System.NotImplementedException($"Invalid exit {direction}")
            };
        }
    }

    [System.Flags]
    public enum Exit
    {
        None = 0,

        Up = 1,
        Down = 2,
        Left = 4,
        Right = 8
    }

    public record ExitPair
    {
        public Exit E1;
        public Exit E2;
    }
}
