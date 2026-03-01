// /Assets/Scripts/Editor/Utilities/RagdollPresetCreator.cs
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace PhoenixRealm.RagdollCreatorPro.Editor
{
    public static class RagdollPresetCreator
    {
        public static void CreatePresetFromCurrentChains(List<CustomChain> chains, string savePath = null)
        {
            if (chains == null || chains.Count == 0)
            {
                EditorUtility.DisplayDialog("Cannot Create Preset",
                    "No chains found to create preset from.", "OK");
                return;
            }

            if (string.IsNullOrEmpty(savePath))
            {
                savePath = EditorUtility.SaveFilePanelInProject(
                    "Save Ragdoll Preset",
                    "NewRagdollPreset",
                    "asset",
                    "Choose where to save the ragdoll preset"
                );
            }

            if (string.IsNullOrEmpty(savePath))
                return;

            var preset = ScriptableObject.CreateInstance<RagdollHumanoidPreset>();
            preset.PresetName = System.IO.Path.GetFileNameWithoutExtension(savePath);
            preset.Description = $"Preset created from {chains.Count} chain(s)";
            preset.CaptureFromChains(chains);

            AssetDatabase.CreateAsset(preset, savePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = preset;

            Debug.Log($"[Ragdoll Preset] Created preset '{preset.PresetName}' with {chains.Count} chains at {savePath}");
        }

        public static T LoadPreset<T>(string path) where T : RagdollPresetBase
        {
            var preset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (preset == null)
            {
                Debug.LogError($"[Ragdoll Preset] Failed to load preset from {path}");
            }
            return preset;
        }

        public static List<T> FindAllPresetsOfType<T>() where T : RagdollPresetBase
        {
            var presets = new List<T>();
            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var preset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (preset != null)
                {
                    presets.Add(preset);
                }
            }

            return presets;
        }

        public static List<RagdollPresetBase> FindAllPresets()
        {
            var presets = new List<RagdollPresetBase>();
            var guids = AssetDatabase.FindAssets($"t:RagdollPresetBase");

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var preset = AssetDatabase.LoadAssetAtPath<RagdollPresetBase>(path);
                if (preset != null)
                {
                    presets.Add(preset);
                }
            }

            return presets;
        }
    }
}
#endif
