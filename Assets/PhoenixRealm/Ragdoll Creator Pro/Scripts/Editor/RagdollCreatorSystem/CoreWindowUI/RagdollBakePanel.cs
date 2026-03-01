#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace PhoenixRealm.RagdollCreatorPro.Editor
{
    public sealed class RagdollBakePanel
    {
        private const float BUTTON_HEIGHT = 25f;

        public void Draw(RagdollMakerContext ctx)
        {
            EditorGUILayout.LabelField("Bake", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            EditorGUILayout.HelpBox("Baking will create Rigidbodies, Colliders, and CharacterJoints based on your chain setup. Existing colliders will be replaced.", MessageType.Info);

            EditorGUILayout.Space(6);
            GUI.enabled = ctx.Validation == null || !ctx.Validation.HasErrors;
            if (GUILayout.Button("Bake RagdollCreatorPro", GUILayout.Height(BUTTON_HEIGHT)))
                RagdollMakerBake.BakeRagdoll(ctx.Chains);
            GUI.enabled = true;
        }
    }
}
#endif
