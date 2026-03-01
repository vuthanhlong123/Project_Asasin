using UnityEngine;

namespace PhoenixRealm.RagdollCreatorPro
{
    [CreateAssetMenu(fileName = "New Humanoid Ragdoll Preset", menuName = "Ragdoll Maker/Presets/Humanoid Preset")]
    public class RagdollHumanoidPreset : RagdollPresetBase
    {
        public RagdollHumanoidPreset()
        {
            PresetName = "Humanoid Ragdoll";
            Description = "Standard humanoid character ragdoll configuration with head, torso, arms, and legs.";
        }
    }
}
