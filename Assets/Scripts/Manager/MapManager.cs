using LD59.Map;
using UnityEngine;

namespace LD59.Manager
{
    public class MapManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject _railPrefab;

        private void Start()
        {
            var go = Instantiate(_railPrefab, new Vector2(0, -7) * (GridManager.GridSize / 100f), Quaternion.identity);
            var rail = go.GetComponent<Rail>();
            var platform = SpriteManager.Instance.GetPlatform();
            rail.SR.sprite = platform.Sprite;
            rail.Exits = platform.Exits;
            rail.CanOverrides = false;
            GridManager.Instance.Register(new(0, -7), rail);
        }
    }
}
