using System;
using TMPro;
using UnityEngine;

namespace Asasingame.Core.Timing
{
    public class GameTimeCounter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;

        private void Update()
        {
            text.text = MathF.Round(Time.time, 2).ToString();
        }
    }
}
