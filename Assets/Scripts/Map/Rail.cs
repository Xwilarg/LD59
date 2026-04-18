using LD59.Manager;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace LD59.Map
{
    public class Rail : MonoBehaviour
    {
        [SerializeField]
        private LineRenderer _lineHint;

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

        private Exit _path = Exit.None;

        public bool IsHint { set; get; } = true;

        public bool CanOverrides { set; get; } = true;
        public Platform Platform { set; get; }

        public Signal Signal { set; get; }

        public SpriteRenderer SR { private set; get; }

        private void Awake()
        {
            SR = GetComponent<SpriteRenderer>();
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
                var list = ExitFlagToList(_path);
                Assert.IsTrue(list.Length == 2, $"Rail path doesn't isn't a line, value contained: {_path} (list values: {string.Join(", ", list)})");

                var pos1 = (Vector3)((Vector2)transform.position * 2f + ((Vector2)GetDirection(list[0]) * GridManager.GridWorld)) / 2f;
                var pos2 = (Vector3)((Vector2)transform.position * 2f + ((Vector2)GetDirection(list[1]) * GridManager.GridWorld)) / 2f;
                _lineHint.SetPositions(new[] { pos1, pos2 });
            }
            else
            {
                _lineHint.gameObject.SetActive(false);
                if (_path == Exit.None)
                {
                    _path = Exits;
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
            if (source != Exit.Up && Exits.HasFlag(Exit.Up)) return Exit.Up;
            if (source != Exit.Down && Exits.HasFlag(Exit.Down)) return Exit.Down;
            if (source != Exit.Left && Exits.HasFlag(Exit.Left)) return Exit.Left;
            return Exit.Right;
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
}
