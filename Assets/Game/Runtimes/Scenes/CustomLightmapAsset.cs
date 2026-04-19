using UnityEngine;

namespace Game.Runtimes.Scenes
{
    [System.Serializable]
    public class LightmapInfo
    {
        public Texture2D lightmapColor;
        public Texture2D lightmapDir;
        public Texture2D shadowMask;
    }

    [CreateAssetMenu(fileName = "Scene Lightmap Asset", menuName = "Game/Scenes/Lighmap Asset")]
    public class CustomLightmapAsset : ScriptableObject
    {
        [SerializeField] private LightmapInfo[] lightmapInfos;

        public LightmapInfo[] LightMaps => lightmapInfos;

        public void SetLightmapInfos(LightmapInfo[] lightmapInfos)
        {
            this.lightmapInfos = lightmapInfos;
        }
    }
}


