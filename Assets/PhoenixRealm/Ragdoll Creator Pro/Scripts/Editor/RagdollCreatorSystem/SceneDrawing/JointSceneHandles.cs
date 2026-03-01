#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace PhoenixRealm.RagdollCreatorPro.Editor
{
    /// <summary>
    /// Static class for drawing joint-related scene handles and gizmos.
    /// Separated from UI to maintain clear separation between scene drawing and UI Toolkit overlay.
    /// </summary>
    public static class JointSceneHandles
    {
        public static void DrawSceneHandles(CustomNode node, Transform bone, JointTool tool, bool xray, RagdollMakerContext ctx)
        {
            if (node == null || bone == null || ctx == null) return;

            Handles.color = new Color(1f, 1f, 1f, xray ? 1f : 0.9f);
            Handles.zTest = xray ? UnityEngine.Rendering.CompareFunction.Always : UnityEngine.Rendering.CompareFunction.LessEqual;

            switch (tool)
            {
                case JointTool.Anchor:
                    {
                        var anchor = node.JointAnchorLocal;
                        Handles.color = new Color(0.22f, 0.8f, 1f, 1f);
                        EditorGUI.BeginChangeCheck();
                        if (JointHandles.DrawAnchorHandle(bone, ref anchor))
                        {
                            if (EditorGUI.EndChangeCheck())
                            {
                                Undo.RecordObject(ctx, "Move Joint Anchor");
                                node.JointAnchorLocal = anchor;
                                ctx.NotifyNodeModified(node);
                                EditorUtility.SetDirty(ctx);
                            }
                        }
                        break;
                    }
                case JointTool.Axis:
                    {
                        var axis = node.JointAxisLocal.sqrMagnitude > 1e-6f ? node.JointAxisLocal : Vector3.right;
                        Handles.color = new Color(1f, 0.92f, 0.2f, 1f);
                        if (JointHandles.DrawAxisDial(bone, ref axis))
                        {
                            Undo.RecordObject(ctx, "Adjust Joint Axis");
                            node.JointAxisLocal = axis;
                            ctx.NotifyNodeModified(node);
                            EditorUtility.SetDirty(ctx);
                        }
                        break;
                    }
                case JointTool.Twist:
                    {
                        // Convert to Unity structs for handles
                        var low = node.JointLimits.lowTwistLimit.ToUnityLimit();
                        var high = node.JointLimits.highTwistLimit.ToUnityLimit();

                        EditorGUI.BeginChangeCheck();
                        if (JointHandles.DrawTwistDial(bone, node.JointAxisLocal, ref low, ref high))
                        {
                            if (EditorGUI.EndChangeCheck())
                            {
                                Undo.RecordObject(ctx, "Adjust Twist Limits");
                                // Update the custom limits with new values
                                var newLimits = node.JointLimits.Clone();
                                newLimits.lowTwistLimit = CustomSoftJointLimit.FromUnityLimit(low);
                                newLimits.highTwistLimit = CustomSoftJointLimit.FromUnityLimit(high);
                                node.JointLimits = newLimits;
                                ctx.NotifyNodeModified(node);
                                EditorUtility.SetDirty(ctx);
                            }
                        }
                        break;
                    }
                case JointTool.Swing:
                    {
                        // Convert to Unity structs for handles
                        var s1 = node.JointLimits.swing1Limit.ToUnityLimit();
                        var s2 = node.JointLimits.swing2Limit.ToUnityLimit();

                        EditorGUI.BeginChangeCheck();
                        if (JointHandles.DrawSwingEnvelope(bone, node.JointAxisLocal, ref s1, ref s2))
                        {
                            if (EditorGUI.EndChangeCheck())
                            {
                                Undo.RecordObject(ctx, "Adjust Swing Limits");
                                // Update the custom limits with new values
                                var newLimits = node.JointLimits.Clone();
                                newLimits.swing1Limit = CustomSoftJointLimit.FromUnityLimit(s1);
                                newLimits.swing2Limit = CustomSoftJointLimit.FromUnityLimit(s2);
                                node.JointLimits = newLimits;
                                ctx.NotifyNodeModified(node);
                                EditorUtility.SetDirty(ctx);
                            }
                        }
                        break;
                    }
            }

            Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
            Handles.color = Color.white;
        }
    }
}
#endif
