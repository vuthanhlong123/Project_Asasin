#if UNITY_EDITOR
using Game.Runtimes.Scenes;
using Unity.Rendering;
using UnityEditor;
using UnityEngine;

public class LightmapExporter
{
    [MenuItem("Tools/Export Lightmaps")]
    static void Export()
    {
        var lmAsset = ScriptableObject.CreateInstance<CustomLightmapAsset>();
        var baked = LightmapSettings.lightmaps;

        var lightMaps = new LightmapInfo[baked.Length];
        for (int i = 0; i < baked.Length; i++)
        {
            lightMaps[i] = new LightmapInfo
            {
                lightmapColor = baked[i].lightmapColor,
                lightmapDir = baked[i].lightmapDir,
                shadowMask = baked[i].shadowMask
            };
        }

        lmAsset.SetLightmapInfos(lightMaps);
        AssetDatabase.CreateAsset(lmAsset, $"Assets/Game/LightmapAsset/{Lightmapping.lightingDataAsset.name}.asset");
        AssetDatabase.SaveAssets();
    }
}
#endif