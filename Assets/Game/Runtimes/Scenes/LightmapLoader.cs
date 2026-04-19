using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEditor;

namespace Game.Runtimes.Scenes
{
    public class LightmapLoader : MonoBehaviour
    {
        [SerializeField] private CustomLightmapAsset lightmapAsset;

        void OnEnable()
        {
            if (lightmapAsset != null && lightmapAsset.LightMaps.Length > 0)
            {
                LightmapData[] lmData = new LightmapData[lightmapAsset.LightMaps.Length];
                for (int i = 0; i < lmData.Length; i++)
                {
                    lmData[i] = new LightmapData
                    {
                        lightmapColor = lightmapAsset.LightMaps[i].lightmapColor,
                        lightmapDir = lightmapAsset.LightMaps[i].lightmapDir,
                        shadowMask = lightmapAsset.LightMaps[i].shadowMask
                    };
                }

                LightmapSettings.lightmaps = lmData;
                DynamicGI.UpdateEnvironment();
            }
        }
    }
}


