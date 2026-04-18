using UnityEngine;

namespace LD59.Map
{
    public class Rail : MonoBehaviour
    {
        public Exit Exits { set; get; }
        public bool CanOverrides { set; get; } = true;

        public SpriteRenderer SR { private set; get; }

        private void Awake()
        {
            SR = GetComponent<SpriteRenderer>();
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
