using TMPro;
using UnityEngine;

namespace LD59.VN
{
    public class TrainPopup : MonoBehaviour
    {
        [SerializeField]
        public TMP_Text _label, _description;

        public void SetLabel(string text) => _label.text = text;
        public void SetDescription(string text) => _description.text = text;
    }
}
