using UnityEngine;

namespace LD59.Manager
{
    public class DebugManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject _debugMenu;

        private void Awake()
        {
#if !UNITY_EDITOR
            _debugMenu.SetActive(false);
#endif
        }

        public void SpawnTrain()
        {

        }
    }
}
