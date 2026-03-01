#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace PhoenixRealm.RagdollCreatorPro.Editor
{
    internal class HitboxSceneDrawer
    {
        #region Vars + Properties

        private static readonly Color PREVIEW_COLOR = new Color(0.3f, 0.8f, 1f, 0.35f);
        private static readonly Color PREVIEW_WIRE_COLOR = new Color(0.3f, 0.8f, 1f, 0.8f);

        #endregion

        #region Custom Functions

        public void DrawHitboxPreviews(System.Collections.Generic.List<HitboxSetupWindow.HitboxPreview> previews)
        {
            if (previews == null || previews.Count == 0)
                return;

            PhoenixRealm.RagdollCreatorPro.RagdollGizmosDrawer.EnsureInit();

            foreach (var preview in previews)
            {
                if (preview.TargetBone == null)
                    continue;

                DrawHitboxPreview(preview);
            }
        }

        private void DrawHitboxPreview(HitboxSetupWindow.HitboxPreview preview)
        {
            Transform bone = preview.TargetBone;
            Vector3 worldPos = bone.TransformPoint(preview.LocalOffset);
            Quaternion worldRot = bone.rotation;

            switch (preview.ColliderType)
            {
                case ColliderType.Sphere:
                    DrawSpherePreview(worldPos, worldRot, preview.ColliderRadius);
                    break;

                case ColliderType.Capsule:
                    DrawCapsulePreview(worldPos, worldRot, preview.ColliderRadius, preview.ColliderHeight, preview.CapsuleDirection);
                    break;

                case ColliderType.Box:
                    DrawBoxPreview(worldPos, worldRot, preview.ColliderSize);
                    break;
            }
        }

        private void DrawSpherePreview(Vector3 center, Quaternion rotation, float radius)
        {
            PhoenixRealm.RagdollCreatorPro.RagdollGizmosDrawer.DrawSphereSolid(center, rotation, radius, PREVIEW_COLOR, false);

            Handles.color = PREVIEW_WIRE_COLOR;
            Handles.DrawWireDisc(center, Vector3.up, radius);
            Handles.DrawWireDisc(center, Vector3.right, radius);
            Handles.DrawWireDisc(center, Vector3.forward, radius);
        }

        private void DrawCapsulePreview(Vector3 center, Quaternion rotation, float radius, float height, CapsuleDirection direction)
        {
            Quaternion adjustedRot = rotation;

            switch (direction)
            {
                case CapsuleDirection.X:
                    adjustedRot *= Quaternion.Euler(0, 0, 90);
                    break;
                case CapsuleDirection.Y:
                    break;
                case CapsuleDirection.Z:
                    adjustedRot *= Quaternion.Euler(90, 0, 0);
                    break;
            }

            PhoenixRealm.RagdollCreatorPro.RagdollGizmosDrawer.DrawCapsuleSolid(center, adjustedRot, radius, height, PREVIEW_COLOR, false);
            PhoenixRealm.RagdollCreatorPro.RagdollGizmosDrawer.DrawCapsuleWire(center, adjustedRot, radius, height, PREVIEW_WIRE_COLOR, 1f);
        }

        private void DrawBoxPreview(Vector3 center, Quaternion rotation, Vector3 size)
        {
            PhoenixRealm.RagdollCreatorPro.RagdollGizmosDrawer.DrawBoxSolid(center, rotation, size, PREVIEW_COLOR, false);

            Handles.color = PREVIEW_WIRE_COLOR;
            Matrix4x4 matrix = Matrix4x4.TRS(center, rotation, size);
            using (new Handles.DrawingScope(matrix))
            {
                Handles.DrawWireCube(Vector3.zero, Vector3.one);
            }
        }

        #endregion
    }
}
#endif
