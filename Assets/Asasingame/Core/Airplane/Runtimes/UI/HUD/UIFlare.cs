using UnityEngine;
using UnityEngine.UI;

namespace Asasingame.Core.Airplane.Runtimes.UIs
{
    public class UIFlare : MonoBehaviour
    {
        [SerializeField] private Image image_Available;

        public void SetAvailable()
        {
            image_Available.enabled = true; 
        }

        public void SetUnAvailable()
        {
            image_Available.enabled = false;
        }
    }
}


