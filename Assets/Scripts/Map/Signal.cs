using UnityEngine;

namespace LD59.Map
{
    public class Signal : MonoBehaviour
    {
        public SpriteRenderer SR { private set; get; }

        public bool IsGreen { set; get; } = false;

        private void Awake()
        {
            SR = GetComponent<SpriteRenderer>();
        }
    }
}
