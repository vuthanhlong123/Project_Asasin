using UnityEngine;

namespace Asasingame.Core.Airplane.Runtimes.Data
{
    [CreateAssetMenu(fileName = "Weapon Unit Data", menuName = "Asasingame/Airplane/Weapon Unit Data")]
    public class WeaponUnitData : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private Sprite icon;
        [SerializeField] private BulletMode bulletMode;
        [SerializeField] private int bulletAmount;

        public string ID => id;
        public Sprite Icon => icon;
        public BulletMode BulletMode => bulletMode;
        public int BulletAmount => bulletAmount;
    }

    public enum BulletMode
    {
        Limit,
        Unlimit
    }
}

