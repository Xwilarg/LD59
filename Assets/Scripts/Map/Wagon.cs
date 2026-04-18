using UnityEngine;

namespace LD59.Map
{
    public class Wagon : MonoBehaviour
    {
        [SerializeField]
        public Transform _forward, _backward;

        public Vector2 TilePos { set; get; }
        public Exit Direction { set; get; }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(_forward.position, _backward.position);
        }
    }
}
