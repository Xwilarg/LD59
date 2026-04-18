using LD59.Manager;
using UnityEngine;

namespace LD59.Map
{
    public class Rail : MonoBehaviour
    {
        public Exit Exits { set; get; }
        public bool CanOverrides { set; get; } = true;
        public Platform Platform { set; get; }

        public SpriteRenderer SR { private set; get; }

        private void Awake()
        {
            SR = GetComponent<SpriteRenderer>();
        }

        public Exit GetExit(Exit source)
        {
            if (source != Exit.Up && Exits.HasFlag(Exit.Up)) return Exit.Up;
            if (source != Exit.Down && Exits.HasFlag(Exit.Down)) return Exit.Down;
            if (source != Exit.Left && Exits.HasFlag(Exit.Left)) return Exit.Left;
            return Exit.Right;
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
