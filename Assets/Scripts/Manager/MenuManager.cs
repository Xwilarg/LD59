using UnityEngine;
using UnityEngine.SceneManagement;

namespace LD59.Manager
{
    public class MenuManager : MonoBehaviour
    {
        public void Play()
        {
            SceneManager.LoadScene("Main");
        }
    }
}
