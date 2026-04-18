using UnityEngine;

namespace LD59.Map
{
    public class Rail : MonoBehaviour
    {
        private SpriteRenderer _sr;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
        }
    }

    public enum Exit
    {
        Up, Down, Left, Right
    }
}
