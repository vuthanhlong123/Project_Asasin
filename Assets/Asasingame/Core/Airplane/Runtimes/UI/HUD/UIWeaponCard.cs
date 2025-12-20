using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Asasingame.Core.Airplane.Runtimes.UIs
{
    public class UIWeaponCard : MonoBehaviour
    {
        [SerializeField] private Image image_WeaponIcon;
        [SerializeField] private TextMeshProUGUI text_ActiveKey;

        public void SetValue(Sprite icon, string key)
        {
            image_WeaponIcon.sprite = icon;
            text_ActiveKey.text = key;
        }

    }
}


