using UnityEngine;

namespace Game.Runtimes.Ultility
{
    public class GameUltility
    {
        public static bool IsInLayerMask(int layer, LayerMask layerMask)
        {
            return (layerMask.value & (1 << layer)) != 0;
        }
    }
}



