using Asasingame.Core.Airplane.Runtimes.Data;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Asasingame.Core.Airplane.Runtimes.UIs
{
    public class UIWeaponManager : MonoBehaviour
    {

        [SerializeField] private UIWeaponCardHolder cardHolder;
        [SerializeField] private UICurrentWeaponActivate weaponActivate;

        public void Initialize(List<UIWeaponDrawData> drawDatas)
        {
            cardHolder.GenerateCards(drawDatas);
            weaponActivate.Hide();
        }

        public void ShowWeaponActivate(Sprite icon, int bullet, BulletMode bulletMode)
        {
            weaponActivate.SetIcon(icon);
            if(bulletMode == BulletMode.Limit)
                weaponActivate.SetBulletRemainingText(bullet.ToString());
            else if(bulletMode == BulletMode.Unlimit)
            {
                weaponActivate.SetBulletRemainingText("--");
            }
            weaponActivate.Show();
        }

        public void UpdateWeaponBullet(int bullet, BulletMode bulletMode)
        {
            if (bulletMode == BulletMode.Limit)
                weaponActivate.SetBulletRemainingText(bullet.ToString());
            else if (bulletMode == BulletMode.Unlimit)
            {
                weaponActivate.SetBulletRemainingText("--");
            }
        }


        [Serializable]
        public class UIWeaponCardHolder
        {
            [SerializeField] private UIWeaponCard cardSample;
            [SerializeField] private Transform container;

            public void GenerateCards(List<UIWeaponDrawData> drawDatas)
            {
                foreach (UIWeaponDrawData data in drawDatas)
                {
                    UIWeaponCard card = Instantiate(cardSample, container);
                    card.SetValue(data.icon, data.symbolKey);
                }
            }
        }

        [Serializable]
        public class UICurrentWeaponActivate
        {
            [SerializeField] private GameObject container;
            [SerializeField] private Image image_Icon;
            [SerializeField] private TextMeshProUGUI text_BulletRemaining;

            public void Show()
            {
                container.SetActive(true);
            }

            public void Hide()
            {
                container.SetActive(false);
            }

            public void SetIcon(Sprite sprite)
            {
                image_Icon.sprite = sprite;
            }

            public void SetBulletRemainingText(string text)
            {
                text_BulletRemaining.text = text;
            }
        }
    }

    [Serializable]
    public class UIWeaponDrawData
    {
        public Sprite icon;
        public string symbolKey;
    }
}


