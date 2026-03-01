#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using PhoenixRealm.RagdollCreatorPro;

namespace PhoenixRealm.RagdollCreatorPro.Editor
{
    public sealed class RagdollSceneDrawer
    {
        private readonly Color m_fillUnselected = new Color(0.20f, 0.80f, 1f, 0.22f);
        private readonly Color m_fillSelected = new Color(1.00f, 0.90f, 0.10f, 0.38f);

        #region Entry
        public void DrawScene(RagdollMakerContext ctx)
        {
            if (ctx == null || ctx.Chains == null || ctx.Chains.Count == 0) return;

            if (EditorApplication.isPlaying)
            {
                DrawPlaymodeIndicator();
            }

            if (RagdollSceneSelection.TryGetSelected(out var sc, out var sn))
            { ctx.SelectedChain = sc; ctx.SelectedNode = sn; }
            else { ctx.SelectedChain = ctx.SelectedNode = -1; }

            int cIdx = 0;
            foreach (var chain in ctx.Chains)
            {
                if (chain == null || chain.Nodes == null) { cIdx++; continue; }
                int nIdx = 0;
                foreach (var node in chain.Nodes)
                {
                    DrawNodeSolidAndPick(ctx, cIdx, nIdx, node);
                    nIdx++;
                }
                cIdx++;
            }

            if (ctx.ActiveTab == OverlayTab.Joint &&
                ctx.SelectedChain >= 0 && ctx.SelectedChain < ctx.Chains.Count &&
                ctx.Chains[ctx.SelectedChain] != null &&
                ctx.SelectedNode >= 0 && ctx.SelectedNode < ctx.Chains[ctx.SelectedChain].Nodes.Count)
            {
                var n = ctx.Chains[ctx.SelectedChain].Nodes[ctx.SelectedNode];
                if (n != null && n.Transform != null)
                {
                    JointSceneHandles.DrawSceneHandles(n, n.Transform, ctx.ActiveJointTool, ctx.XRay, ctx);
                }
            }

            HandleGlobalEvents(ctx);
        }

        private void HandleGlobalEvents(RagdollMakerContext ctx)
        {
            var e = Event.current;

            if (e.type == EventType.ContextClick)
            {
                if (ctx.SelectedChain >= 0 && ctx.SelectedChain < ctx.Chains.Count &&
                    ctx.SelectedNode >= 0 && ctx.SelectedNode < ctx.Chains[ctx.SelectedChain].Nodes.Count)
                {
                    var chain = ctx.Chains[ctx.SelectedChain];
                    var node = chain.Nodes[ctx.SelectedNode];

                    ShowMassCalculationContextMenu(chain, node, ctx);
                    e.Use();
                }
            }

            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
            {
                RagdollSceneSelection.Clear();
                SceneView.RepaintAll();
            }
        }

        private void DrawPlaymodeIndicator()
        {
            Handles.BeginGUI();

            var rect = new Rect(10, 10, 200, 25);
            GUI.color = new Color(1f, 0.8f, 0.2f, 0.9f);
            GUI.Box(rect, "");

            GUI.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            var labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontStyle = FontStyle.Bold;
            labelStyle.alignment = TextAnchor.MiddleCenter;
            GUI.Label(rect, "RagdollCreatorPro Maker - Playmode Active", labelStyle);

            GUI.color = Color.white;
            Handles.EndGUI();
        }
        #endregion

        #region Helper Methods for Scale-Aware Visualization

        private float GetWorldSpaceSphereRadius(Transform transform, float localRadius)
        {
            Vector3 scale = transform.lossyScale;
            float maxScale = Mathf.Max(Mathf.Abs(scale.x), Mathf.Max(Mathf.Abs(scale.y), Mathf.Abs(scale.z)));
            return localRadius * maxScale;
        }

        private Vector3 GetWorldSpaceBoxSize(Transform transform, Vector3 localSize)
        {
            Vector3 scale = transform.lossyScale;
            return new Vector3(
                localSize.x * Mathf.Abs(scale.x),
                localSize.y * Mathf.Abs(scale.y),
                localSize.z * Mathf.Abs(scale.z)
            );
        }

        private void GetWorldSpaceCapsuleDimensions(Transform transform, float localRadius, float localHeight, CapsuleDirection direction, out float worldRadius, out float worldHeight)
        {
            Vector3 scale = transform.lossyScale;

            switch (direction)
            {
                case CapsuleDirection.X:
                    worldRadius = Mathf.Max(Mathf.Abs(scale.y), Mathf.Abs(scale.z)) * localRadius;
                    worldHeight = Mathf.Abs(scale.x) * localHeight;
                    break;
                case CapsuleDirection.Y:
                    worldRadius = Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z)) * localRadius;
                    worldHeight = Mathf.Abs(scale.y) * localHeight;
                    break;
                case CapsuleDirection.Z:
                    worldRadius = Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y)) * localRadius;
                    worldHeight = Mathf.Abs(scale.z) * localHeight;
                    break;
                default:
                    worldRadius = Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z)) * localRadius;
                    worldHeight = Mathf.Abs(scale.y) * localHeight;
                    break;
            }
        }

        #endregion

        #region Draw node + collider tool routing
        private void DrawNodeSolidAndPick(RagdollMakerContext ctx, int chainIdx, int nodeIdx, CustomNode node)
        {
            if (node == null || node.Transform == null) return;

            Transform bone = node.Transform;
            Vector3 pos = bone.TransformPoint(node.LocalOffset);
            Quaternion rot = bone.rotation;

            bool selected = (ctx.SelectedChain == chainIdx && ctx.SelectedNode == nodeIdx);
            Color fill = selected ? m_fillSelected : m_fillUnselected;

            if (EditorApplication.isPlaying)
            {
                fill = new Color(fill.r, fill.g, fill.b, fill.a * 0.7f);
            }

            switch (node.ColliderType)
            {
                case ColliderType.Sphere:
                    {
                        float localRadius = Mathf.Max(0f, node.ColliderRadius);
                        if (localRadius > 0f)
                        {
                            float worldRadius = GetWorldSpaceSphereRadius(bone, localRadius);
                            RagdollGizmosDrawer.DrawSphereSolid(pos, rot, worldRadius, fill, ctx.XRay);
                            DrawPickHandle(chainIdx, nodeIdx, pos, rot, Mathf.Max(0.07f, worldRadius));
                            using (new Handles.DrawingScope(new Color(1f, 1f, 1f, selected ? 0.9f : 0.25f), Matrix4x4.TRS(pos, rot, Vector3.one)))
                            {
                                if (ctx.ActiveTab == OverlayTab.Collider)
                                {
                                    Handles.DrawWireDisc(Vector3.zero, Vector3.up, worldRadius);
                                    Handles.DrawWireDisc(Vector3.zero, Vector3.right, worldRadius);
                                    Handles.DrawWireDisc(Vector3.zero, Vector3.forward, worldRadius);
                                }
                            }
                        }
                        break;
                    }

                case ColliderType.Capsule:
                    {
                        float localRadius = Mathf.Max(0f, node.ColliderRadius);
                        float localHeight = Mathf.Max(0f, node.ColliderHeight);
                        if (localRadius > 0f)
                        {
                            GetWorldSpaceCapsuleDimensions(bone, localRadius, localHeight, node.CapsuleDirection, out float worldRadius, out float worldHeight);
                            DrawCapsuleWithDirection(pos, rot, worldRadius, worldHeight, node.CapsuleDirection, fill, ctx.XRay);
                            float pickSize = Mathf.Max(0.07f, Mathf.Max(worldRadius, worldHeight * 0.25f));
                            DrawPickHandle(chainIdx, nodeIdx, pos, rot, pickSize);

                            if (ctx.ActiveTab == OverlayTab.Collider)
                                DrawCapsuleWireWithDirection(pos, rot, worldRadius, worldHeight, node.CapsuleDirection, Color.white, selected ? 0.9f : 0.25f);
                        }
                        break;
                    }

                case ColliderType.Box:
                    {
                        Vector3 localSize = node.ColliderSize;
                        if (localSize.sqrMagnitude > 1e-6f)
                        {
                            Vector3 worldSize = GetWorldSpaceBoxSize(bone, localSize);
                            RagdollGizmosDrawer.DrawBoxSolid(pos, rot, worldSize, fill, ctx.XRay);
                            float pickSize = Mathf.Max(worldSize.x, Mathf.Max(worldSize.y, worldSize.z)) * 0.5f;
                            DrawPickHandle(chainIdx, nodeIdx, pos, rot, Mathf.Max(0.07f, pickSize));
                            using (new Handles.DrawingScope(new Color(1f, 1f, 1f, selected ? 0.9f : 0.25f), Matrix4x4.TRS(pos, rot, Vector3.one)))
                                if (ctx.ActiveTab == OverlayTab.Collider)
                                    Handles.DrawWireCube(Vector3.zero, worldSize);
                        }
                        break;
                    }
            }

            if (!selected || ctx.ActiveTab != OverlayTab.Collider || EditorApplication.isPlaying) return;

            HandleContextMenu(ctx, chainIdx, nodeIdx, node);

            var prevTool = Tools.current;
            Tools.current = Tool.None;

            Vector3 originalLocalOffset = node.LocalOffset;
            float originalRadius = node.ColliderRadius;
            float originalHeight = node.ColliderHeight;
            Vector3 originalSize = node.ColliderSize;
            bool nodeWasModified = false;

            switch (ctx.ActiveColliderTool)
            {
                case ColliderTool.Move:
                    {
                        Undo.RecordObject(ctx, "Move Node Offset");
                        Vector3 worldPos = Handles.PositionHandle(pos, rot);
                        if (worldPos != pos)
                        {
                            node.LocalOffset = bone.InverseTransformPoint(worldPos);
                            nodeWasModified = true;
                        }
                        break;
                    }
                case ColliderTool.Rotate:
                    {
                        Handles.BeginGUI();
                        Vector2 screenPos = HandleUtility.WorldToGUIPoint(pos);
                        GUI.color = Color.yellow;
                        GUI.Label(new Rect(screenPos.x + 10, screenPos.y - 60, 300, 20), "Rotation locked to bone rotation");
                        GUI.Label(new Rect(screenPos.x + 10, screenPos.y - 40, 300, 20), "Rotate the bone Transform to change orientation");

                        if (node.ColliderType == ColliderType.Capsule)
                        {
                            GUI.color = Color.cyan;
                            GUI.Label(new Rect(screenPos.x + 10, screenPos.y - 20, 200, 20), $"Capsule Direction: {node.CapsuleDirection}-Axis");
                        }

                        GUI.color = Color.white;
                        Handles.EndGUI();
                        break;
                    }
                case ColliderTool.Scale:
                    {
                        switch (node.ColliderType)
                        {
                            case ColliderType.Sphere:
                                {
                                    Undo.RecordObject(ctx, "Scale Sphere Radius");
                                    float localRadius = Mathf.Max(0.0001f, node.ColliderRadius);
                                    float worldRadius = GetWorldSpaceSphereRadius(bone, localRadius);
                                    float newWorldRadius = Handles.RadiusHandle(rot, pos, worldRadius);
                                    if (!Mathf.Approximately(newWorldRadius, worldRadius))
                                    {
                                        Vector3 scale = bone.lossyScale;
                                        float maxScale = Mathf.Max(Mathf.Abs(scale.x), Mathf.Max(Mathf.Abs(scale.y), Mathf.Abs(scale.z)));
                                        float newLocalRadius = maxScale > 0.0001f ? newWorldRadius / maxScale : newWorldRadius;
                                        node.ColliderRadius = Mathf.Max(0f, newLocalRadius);
                                        nodeWasModified = true;
                                    }
                                    break;
                                }

                            case ColliderType.Box:
                                {
                                    Undo.RecordObject(ctx, "Scale Box Size");

                                    Vector3 tempSize = node.ColliderSize;
                                    Vector3 tempLocalOffset = node.LocalOffset;

                                    bool boxModified = DrawDirectionalBoxScaleHandles(pos, rot, ref tempSize, ref tempLocalOffset, bone);

                                    if (boxModified)
                                    {
                                        node.ColliderSize = tempSize;
                                        node.LocalOffset = tempLocalOffset;
                                        nodeWasModified = true;
                                    }
                                    break;
                                }

                            case ColliderType.Capsule:
                                {
                                    Undo.RecordObject(ctx, "Scale Capsule Params");

                                    float tempRadius = node.ColliderRadius;
                                    float tempHeight = node.ColliderHeight;
                                    Vector3 tempLocalOffset = node.LocalOffset;

                                    bool capsuleModified = DrawDirectionalCapsuleScaleHandles(pos, rot, ref tempRadius, ref tempHeight, ref tempLocalOffset, bone, node.CapsuleDirection);

                                    if (capsuleModified)
                                    {
                                        node.ColliderRadius = tempRadius;
                                        node.ColliderHeight = tempHeight;
                                        node.LocalOffset = tempLocalOffset;
                                        nodeWasModified = true;
                                    }
                                    break;
                                }
                        }
                        break;
                    }
            }

            if (nodeWasModified && RagdollChainSnapper.IsValidColliderNode(node))
            {
                try
                {
                    if (chainIdx >= 0 && chainIdx < ctx.Chains.Count)
                    {
                        var currentChain = ctx.Chains[chainIdx];
                        RagdollChainSnapper.MaintainChildrenOfNode(currentChain.Nodes, node);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"Failed to auto-maintain children of node '{node.Transform.name}': {e.Message}");
                }

                ctx.NotifyNodeModified(node);
                EditorUtility.SetDirty(ctx);
            }

            Tools.current = prevTool;
        }

        private void DrawPickHandle(int chainIdx, int nodeIdx, Vector3 pos, Quaternion rot, float sizeHint)
        {
            float handleSize = HandleUtility.GetHandleSize(pos) * 0.2f + sizeHint * 0.2f;
            Handles.color = Color.clear;

            bool clicked = Handles.Button(pos, rot, handleSize, handleSize, Handles.SphereHandleCap);
            if (clicked)
            {
                if (RagdollSceneSelection.IsSelected(chainIdx, nodeIdx))
                    RagdollSceneSelection.Clear();
                else
                    RagdollSceneSelection.SetSelection(chainIdx, nodeIdx);

                GUI.changed = true;
                SceneView.RepaintAll();
            }
        }

        #region Capsule Direction Visualization

        private void DrawCapsuleWithDirection(Vector3 position, Quaternion boneRotation, float radius, float height, CapsuleDirection direction, Color fillColor, bool xray)
        {
            Quaternion capsuleRotation = GetCapsuleRotationFromDirection(boneRotation, direction);
            RagdollGizmosDrawer.DrawCapsuleSolid(position, capsuleRotation, radius, height, fillColor, xray);
        }

        private void DrawCapsuleWireWithDirection(Vector3 position, Quaternion boneRotation, float radius, float height, CapsuleDirection direction, Color wireColor, float alpha)
        {
            Quaternion capsuleRotation = GetCapsuleRotationFromDirection(boneRotation, direction);
            RagdollGizmosDrawer.DrawCapsuleWire(position, capsuleRotation, radius, height, wireColor, alpha);
        }

        private Quaternion GetCapsuleRotationFromDirection(Quaternion boneRotation, CapsuleDirection direction)
        {
            Quaternion directionRotation = Quaternion.identity;

            switch (direction)
            {
                case CapsuleDirection.X:
                    directionRotation = Quaternion.Euler(0, 0, 90);
                    break;
                case CapsuleDirection.Y:
                    directionRotation = Quaternion.identity;
                    break;
                case CapsuleDirection.Z:
                    directionRotation = Quaternion.Euler(90, 0, 0);
                    break;
            }

            return boneRotation * directionRotation;
        }

        #endregion

        #region Directional Box Scaling
        private bool DrawDirectionalBoxScaleHandles(Vector3 position, Quaternion rotation, ref Vector3 size, ref Vector3 localOffset, Transform bone)
        {
            bool modified = false;
            float handleSize = HandleUtility.GetHandleSize(position);

            Vector3 scale = bone.lossyScale;
            Vector3 worldSize = GetWorldSpaceBoxSize(bone, size);

            size.x = Mathf.Max(0.001f, size.x);
            size.y = Mathf.Max(0.001f, size.y);
            size.z = Mathf.Max(0.001f, size.z);

            Vector3 right = rotation * Vector3.right;
            Vector3 up = rotation * Vector3.up;
            Vector3 forward = rotation * Vector3.forward;

            Color handleColor = new Color(0.2f, 1f, 0.2f, 0.9f);

            Vector3 rightFace = position + right * (worldSize.x * 0.5f);
            Vector3 leftFace = position - right * (worldSize.x * 0.5f);
            Vector3 topFace = position + up * (worldSize.y * 0.5f);
            Vector3 bottomFace = position - up * (worldSize.y * 0.5f);
            Vector3 frontFace = position + forward * (worldSize.z * 0.5f);
            Vector3 backFace = position - forward * (worldSize.z * 0.5f);

            using (new Handles.DrawingScope(handleColor))
            {
                EditorGUI.BeginChangeCheck();
                Vector3 newRightFace = Handles.Slider(rightFace, right, handleSize * 0.15f, Handles.CubeHandleCap, 0.1f);
                if (EditorGUI.EndChangeCheck())
                {
                    float worldMovement = Vector3.Dot(newRightFace - rightFace, right);
                    float localMovement = Mathf.Abs(scale.x) > 0.0001f ? worldMovement / Mathf.Abs(scale.x) : 0f;
                    float newSizeX = Mathf.Max(0.001f, size.x + localMovement);

                    Vector3 centerShift = right * (worldMovement * 0.5f);
                    Vector3 newWorldCenter = position + centerShift;
                    localOffset = bone.InverseTransformPoint(newWorldCenter);

                    size.x = newSizeX;
                    modified = true;
                }

                EditorGUI.BeginChangeCheck();
                Vector3 newLeftFace = Handles.Slider(leftFace, -right, handleSize * 0.15f, Handles.CubeHandleCap, 0.1f);
                if (EditorGUI.EndChangeCheck())
                {
                    float worldMovement = Vector3.Dot(leftFace - newLeftFace, right);
                    float localMovement = Mathf.Abs(scale.x) > 0.0001f ? worldMovement / Mathf.Abs(scale.x) : 0f;
                    float newSizeX = Mathf.Max(0.001f, size.x + localMovement);

                    Vector3 centerShift = -right * (worldMovement * 0.5f);
                    Vector3 newWorldCenter = position + centerShift;
                    localOffset = bone.InverseTransformPoint(newWorldCenter);

                    size.x = newSizeX;
                    modified = true;
                }

                EditorGUI.BeginChangeCheck();
                Vector3 newTopFace = Handles.Slider(topFace, up, handleSize * 0.15f, Handles.CubeHandleCap, 0.1f);
                if (EditorGUI.EndChangeCheck())
                {
                    float worldMovement = Vector3.Dot(newTopFace - topFace, up);
                    float localMovement = Mathf.Abs(scale.y) > 0.0001f ? worldMovement / Mathf.Abs(scale.y) : 0f;
                    float newSizeY = Mathf.Max(0.001f, size.y + localMovement);

                    Vector3 centerShift = up * (worldMovement * 0.5f);
                    Vector3 newWorldCenter = position + centerShift;
                    localOffset = bone.InverseTransformPoint(newWorldCenter);

                    size.y = newSizeY;
                    modified = true;
                }

                EditorGUI.BeginChangeCheck();
                Vector3 newBottomFace = Handles.Slider(bottomFace, -up, handleSize * 0.15f, Handles.CubeHandleCap, 0.1f);
                if (EditorGUI.EndChangeCheck())
                {
                    float worldMovement = Vector3.Dot(bottomFace - newBottomFace, up);
                    float localMovement = Mathf.Abs(scale.y) > 0.0001f ? worldMovement / Mathf.Abs(scale.y) : 0f;
                    float newSizeY = Mathf.Max(0.001f, size.y + localMovement);

                    Vector3 centerShift = -up * (worldMovement * 0.5f);
                    Vector3 newWorldCenter = position + centerShift;
                    localOffset = bone.InverseTransformPoint(newWorldCenter);

                    size.y = newSizeY;
                    modified = true;
                }

                EditorGUI.BeginChangeCheck();
                Vector3 newFrontFace = Handles.Slider(frontFace, forward, handleSize * 0.15f, Handles.CubeHandleCap, 0.1f);
                if (EditorGUI.EndChangeCheck())
                {
                    float worldMovement = Vector3.Dot(newFrontFace - frontFace, forward);
                    float localMovement = Mathf.Abs(scale.z) > 0.0001f ? worldMovement / Mathf.Abs(scale.z) : 0f;
                    float newSizeZ = Mathf.Max(0.001f, size.z + localMovement);

                    Vector3 centerShift = forward * (worldMovement * 0.5f);
                    Vector3 newWorldCenter = position + centerShift;
                    localOffset = bone.InverseTransformPoint(newWorldCenter);

                    size.z = newSizeZ;
                    modified = true;
                }

                EditorGUI.BeginChangeCheck();
                Vector3 newBackFace = Handles.Slider(backFace, -forward, handleSize * 0.15f, Handles.CubeHandleCap, 0.1f);
                if (EditorGUI.EndChangeCheck())
                {
                    float worldMovement = Vector3.Dot(backFace - newBackFace, forward);
                    float localMovement = Mathf.Abs(scale.z) > 0.0001f ? worldMovement / Mathf.Abs(scale.z) : 0f;
                    float newSizeZ = Mathf.Max(0.001f, size.z + localMovement);

                    Vector3 centerShift = -forward * (worldMovement * 0.5f);
                    Vector3 newWorldCenter = position + centerShift;
                    localOffset = bone.InverseTransformPoint(newWorldCenter);

                    size.z = newSizeZ;
                    modified = true;
                }
            }

            if (modified)
            {
                DrawBoxScalingGuides(position, rotation, GetWorldSpaceBoxSize(bone, size), handleSize);
            }

            return modified;
        }

        private void DrawBoxScalingGuides(Vector3 position, Quaternion rotation, Vector3 size, float handleSize)
        {
            Vector3 right = rotation * Vector3.right;
            Vector3 up = rotation * Vector3.up;
            Vector3 forward = rotation * Vector3.forward;

            Vector3 halfSize = size * 0.5f;

            using (new Handles.DrawingScope(new Color(0.2f, 1f, 0.2f, 0.7f)))
            {
                Vector3 xLineOffset = up * (halfSize.y + handleSize * 0.2f) + forward * (halfSize.z + handleSize * 0.1f);
                Vector3 xLeft = position - right * halfSize.x + xLineOffset;
                Vector3 xRight = position + right * halfSize.x + xLineOffset;

                Handles.DrawLine(xLeft, xRight);
                Handles.DrawLine(xLeft - up * handleSize * 0.03f, xLeft + up * handleSize * 0.03f);
                Handles.DrawLine(xRight - up * handleSize * 0.03f, xRight + up * handleSize * 0.03f);

                Vector3 yLineOffset = right * (halfSize.x + handleSize * 0.2f) + forward * (halfSize.z + handleSize * 0.1f);
                Vector3 yBottom = position - up * halfSize.y + yLineOffset;
                Vector3 yTop = position + up * halfSize.y + yLineOffset;

                Handles.DrawLine(yBottom, yTop);
                Handles.DrawLine(yBottom - right * handleSize * 0.03f, yBottom + right * handleSize * 0.03f);
                Handles.DrawLine(yTop - right * handleSize * 0.03f, yTop + right * handleSize * 0.03f);

                Vector3 zLineOffset = right * (halfSize.x + handleSize * 0.1f) + up * (halfSize.y + handleSize * 0.2f);
                Vector3 zBack = position - forward * halfSize.z + zLineOffset;
                Vector3 zFront = position + forward * halfSize.z + zLineOffset;

                Handles.DrawLine(zBack, zFront);
                Handles.DrawLine(zBack - up * handleSize * 0.03f, zBack + up * handleSize * 0.03f);
                Handles.DrawLine(zFront - up * handleSize * 0.03f, zFront + up * handleSize * 0.03f);
            }

            using (new Handles.DrawingScope(new Color(1f, 1f, 0.2f, 0.8f)))
            {
                Vector3[] facePositions = {
                    position + right * halfSize.x,
                    position - right * halfSize.x,
                    position + up * halfSize.y,
                    position - up * halfSize.y,
                    position + forward * halfSize.z,
                    position - forward * halfSize.z
                };

                foreach (var facePos in facePositions)
                {
                    Handles.DrawWireDisc(facePos, (facePos - position).normalized, handleSize * 0.02f);
                }
            }

            Handles.BeginGUI();
            Vector3 xLabelPos = position + up * (halfSize.y + handleSize * 0.3f) + forward * (halfSize.z + handleSize * 0.1f);
            Vector3 yLabelPos = position + right * (halfSize.x + handleSize * 0.3f) + forward * (halfSize.z + handleSize * 0.1f);
            Vector3 zLabelPos = position + right * (halfSize.x + handleSize * 0.1f) + up * (halfSize.y + handleSize * 0.3f);

            Vector2 xScreenPos = HandleUtility.WorldToGUIPoint(xLabelPos);
            Vector2 yScreenPos = HandleUtility.WorldToGUIPoint(yLabelPos);
            Vector2 zScreenPos = HandleUtility.WorldToGUIPoint(zLabelPos);

            GUI.color = new Color(0.2f, 1f, 0.2f, 0.9f);
            GUI.Label(new Rect(xScreenPos.x + 5, xScreenPos.y - 8, 120, 16), $"W: {size.x:F2}");
            GUI.Label(new Rect(yScreenPos.x + 5, yScreenPos.y - 8, 120, 16), $"H: {size.y:F2}");
            GUI.Label(new Rect(zScreenPos.x + 5, zScreenPos.y - 8, 120, 16), $"D: {size.z:F2}");

            GUI.color = Color.white;
            Handles.EndGUI();
        }
        #endregion

        #region Directional Capsule Scaling
        private bool DrawDirectionalCapsuleScaleHandles(Vector3 position, Quaternion rotation, ref float radius, ref float height, ref Vector3 localOffset, Transform bone, CapsuleDirection direction)
        {
            bool modified = false;
            float handleSize = HandleUtility.GetHandleSize(position);

            Vector3 scale = bone.lossyScale;
            GetWorldSpaceCapsuleDimensions(bone, radius, height, direction, out float worldRadius, out float worldHeight);

            radius = Mathf.Max(0.001f, radius);
            height = Mathf.Max(0f, height);

            float totalWorldHeight = worldHeight + worldRadius * 2f;
            float halfWorldHeight = totalWorldHeight * 0.5f;

            Vector3 heightAxis = GetCapsuleHeightAxis(rotation, direction);
            Vector3 rightAxis, forwardAxis;
            GetCapsulePerpendicularAxes(rotation, direction, out rightAxis, out forwardAxis);

            Color handleColor = new Color(0.2f, 1f, 0.2f, 0.9f);

            Vector3 topPos = position + heightAxis * halfWorldHeight;
            Vector3 bottomPos = position - heightAxis * halfWorldHeight;

            using (new Handles.DrawingScope(handleColor))
            {
                EditorGUI.BeginChangeCheck();
                Vector3 newTopPos = Handles.Slider(topPos, heightAxis, handleSize * 0.15f, Handles.ConeHandleCap, 0.1f);
                if (EditorGUI.EndChangeCheck())
                {
                    float worldMovement = Vector3.Dot(newTopPos - topPos, heightAxis);
                    float heightScale = GetCapsuleHeightScale(scale, direction);
                    float localMovement = heightScale > 0.0001f ? worldMovement / heightScale : 0f;

                    float newWorldTotalHeight = totalWorldHeight + worldMovement;
                    newWorldTotalHeight = Mathf.Max(worldRadius * 2f, newWorldTotalHeight);
                    float newLocalHeight = Mathf.Max(0f, (newWorldTotalHeight - worldRadius * 2f) / (heightScale > 0.0001f ? heightScale : 1f));

                    Vector3 centerShift = heightAxis * (worldMovement * 0.5f);
                    Vector3 newWorldCenter = position + centerShift;
                    localOffset = bone.InverseTransformPoint(newWorldCenter);

                    height = newLocalHeight;
                    modified = true;
                }

                EditorGUI.BeginChangeCheck();
                Vector3 newBottomPos = Handles.Slider(bottomPos, -heightAxis, handleSize * 0.15f, Handles.ConeHandleCap, 0.1f);
                if (EditorGUI.EndChangeCheck())
                {
                    float worldMovement = Vector3.Dot(bottomPos - newBottomPos, heightAxis);
                    float heightScale = GetCapsuleHeightScale(scale, direction);
                    float localMovement = heightScale > 0.0001f ? worldMovement / heightScale : 0f;

                    float newWorldTotalHeight = totalWorldHeight + worldMovement;
                    newWorldTotalHeight = Mathf.Max(worldRadius * 2f, newWorldTotalHeight);
                    float newLocalHeight = Mathf.Max(0f, (newWorldTotalHeight - worldRadius * 2f) / (heightScale > 0.0001f ? heightScale : 1f));

                    Vector3 centerShift = -heightAxis * (worldMovement * 0.5f);
                    Vector3 newWorldCenter = position + centerShift;
                    localOffset = bone.InverseTransformPoint(newWorldCenter);

                    height = newLocalHeight;
                    modified = true;
                }
            }

            Vector3[] radiusDirections = { rightAxis, -rightAxis, forwardAxis, -forwardAxis };

            using (new Handles.DrawingScope(handleColor))
            {
                for (int i = 0; i < radiusDirections.Length; i++)
                {
                    Vector3 handlePos = position + radiusDirections[i] * worldRadius;

                    EditorGUI.BeginChangeCheck();
                    Vector3 newHandlePos = Handles.Slider(handlePos, radiusDirections[i], handleSize * 0.12f, Handles.SphereHandleCap, 0.05f);
                    if (EditorGUI.EndChangeCheck())
                    {
                        float worldDeltaRadius = Vector3.Dot(newHandlePos - handlePos, radiusDirections[i]);
                        float radiusScale = GetCapsuleRadiusScale(scale, direction);
                        float localDeltaRadius = radiusScale > 0.0001f ? worldDeltaRadius / radiusScale : 0f;
                        float newLocalRadius = Mathf.Max(0.001f, radius + localDeltaRadius);

                        radius = newLocalRadius;
                        GetWorldSpaceCapsuleDimensions(bone, radius, height, direction, out float newWorldRadius, out _);
                        if (height + newWorldRadius * 2f < newWorldRadius * 2f)
                        {
                            height = 0f;
                        }
                        modified = true;
                    }
                }
            }

            if (modified)
            {
                GetWorldSpaceCapsuleDimensions(bone, radius, height, direction, out float displayWorldRadius, out float displayWorldHeight);
                DrawCapsuleScalingGuides(position, rotation, displayWorldRadius, displayWorldHeight, handleSize, direction);
            }

            return modified;
        }

        private float GetCapsuleHeightScale(Vector3 lossyScale, CapsuleDirection direction)
        {
            switch (direction)
            {
                case CapsuleDirection.X: return Mathf.Abs(lossyScale.x);
                case CapsuleDirection.Y: return Mathf.Abs(lossyScale.y);
                case CapsuleDirection.Z: return Mathf.Abs(lossyScale.z);
                default: return Mathf.Abs(lossyScale.y);
            }
        }

        private float GetCapsuleRadiusScale(Vector3 lossyScale, CapsuleDirection direction)
        {
            switch (direction)
            {
                case CapsuleDirection.X: return Mathf.Max(Mathf.Abs(lossyScale.y), Mathf.Abs(lossyScale.z));
                case CapsuleDirection.Y: return Mathf.Max(Mathf.Abs(lossyScale.x), Mathf.Abs(lossyScale.z));
                case CapsuleDirection.Z: return Mathf.Max(Mathf.Abs(lossyScale.x), Mathf.Abs(lossyScale.y));
                default: return Mathf.Max(Mathf.Abs(lossyScale.x), Mathf.Abs(lossyScale.z));
            }
        }

        private Vector3 GetCapsuleHeightAxis(Quaternion rotation, CapsuleDirection direction)
        {
            switch (direction)
            {
                case CapsuleDirection.X:
                    return rotation * Vector3.right;
                case CapsuleDirection.Y:
                    return rotation * Vector3.up;
                case CapsuleDirection.Z:
                    return rotation * Vector3.forward;
                default:
                    return rotation * Vector3.up;
            }
        }

        private void GetCapsulePerpendicularAxes(Quaternion rotation, CapsuleDirection direction, out Vector3 rightAxis, out Vector3 forwardAxis)
        {
            switch (direction)
            {
                case CapsuleDirection.X:
                    rightAxis = rotation * Vector3.up;
                    forwardAxis = rotation * Vector3.forward;
                    break;
                case CapsuleDirection.Y:
                    rightAxis = rotation * Vector3.right;
                    forwardAxis = rotation * Vector3.forward;
                    break;
                case CapsuleDirection.Z:
                    rightAxis = rotation * Vector3.right;
                    forwardAxis = rotation * Vector3.up;
                    break;
                default:
                    rightAxis = rotation * Vector3.right;
                    forwardAxis = rotation * Vector3.forward;
                    break;
            }
        }

        private void DrawCapsuleScalingGuides(Vector3 position, Quaternion rotation, float radius, float height, float handleSize, CapsuleDirection direction)
        {
            Vector3 heightAxis = GetCapsuleHeightAxis(rotation, direction);
            Vector3 rightAxis, forwardAxis;
            GetCapsulePerpendicularAxes(rotation, direction, out rightAxis, out forwardAxis);

            float totalHeight = height + radius * 2f;
            float halfHeight = totalHeight * 0.5f;

            using (new Handles.DrawingScope(new Color(0.2f, 1f, 0.2f, 0.7f)))
            {
                Vector3 heightLineOffset = rightAxis * (radius + handleSize * 0.2f);
                Vector3 topPoint = position + heightAxis * halfHeight + heightLineOffset;
                Vector3 bottomPoint = position - heightAxis * halfHeight + heightLineOffset;

                Handles.DrawLine(topPoint, bottomPoint);
                Handles.DrawLine(topPoint - rightAxis * handleSize * 0.05f, topPoint + rightAxis * handleSize * 0.05f);
                Handles.DrawLine(bottomPoint - rightAxis * handleSize * 0.05f, bottomPoint + rightAxis * handleSize * 0.05f);

                Vector3 radiusLineOffset = heightAxis * (halfHeight + handleSize * 0.2f);
                Vector3 centerPoint = position + radiusLineOffset;
                Vector3 radiusPoint = centerPoint + rightAxis * radius;

                Handles.DrawLine(centerPoint, radiusPoint);
                Handles.DrawLine(centerPoint - heightAxis * handleSize * 0.02f, centerPoint + heightAxis * handleSize * 0.02f);
                Handles.DrawLine(radiusPoint - heightAxis * handleSize * 0.02f, radiusPoint + heightAxis * handleSize * 0.02f);
            }

            Vector3 topAnchor = position + heightAxis * (totalHeight * 0.5f);
            Vector3 bottomAnchor = position - heightAxis * (totalHeight * 0.5f);

            using (new Handles.DrawingScope(new Color(1f, 1f, 0.2f, 0.8f)))
            {
                Handles.DrawWireDisc(topAnchor, heightAxis, handleSize * 0.03f);
                Handles.DrawWireDisc(bottomAnchor, heightAxis, handleSize * 0.03f);
            }

            Handles.BeginGUI();
            Vector3 heightLabelPos = position + rightAxis * (radius + handleSize * 0.3f);
            Vector3 radiusLabelPos = position + heightAxis * (halfHeight + handleSize * 0.3f) + rightAxis * (radius * 0.5f);

            Vector2 heightScreenPos = HandleUtility.WorldToGUIPoint(heightLabelPos);
            Vector2 radiusScreenPos = HandleUtility.WorldToGUIPoint(radiusLabelPos);

            GUI.color = new Color(0.2f, 1f, 0.2f, 0.9f);
            GUI.Label(new Rect(heightScreenPos.x + 5, heightScreenPos.y - 8, 100, 16), $"H: {height:F2}");
            GUI.Label(new Rect(radiusScreenPos.x + 5, radiusScreenPos.y - 8, 100, 16), $"R: {radius:F2}");

            GUI.color = Color.white;
            Handles.EndGUI();
        }
        #endregion

        private void HandleContextMenu(RagdollMakerContext ctx, int chainIdx, int nodeIdx, CustomNode node)
        {
            var e = Event.current;
            if (e.type == EventType.ContextClick)
            {
                if (chainIdx >= 0 && chainIdx < ctx.Chains.Count &&
                    nodeIdx >= 0 && nodeIdx < ctx.Chains[chainIdx].Nodes.Count)
                {
                    var chain = ctx.Chains[chainIdx];
                    ShowMassCalculationContextMenu(chain, node, ctx);
                    e.Use();
                }
            }
        }

        private void ShowMassCalculationContextMenu(CustomChain chain, CustomNode node, RagdollMakerContext ctx)
        {
            GenericMenu menu = new GenericMenu();

            menu.AddDisabledItem(new GUIContent($"Chain: {chain.ChainName}"));
            menu.AddSeparator("");

            menu.AddItem(
                new GUIContent("Auto Calculate Mass for Chain"),
                false,
                () => {
                    if (ctx.TargetCharacter != null)
                    {
                        var ragdollMap = ctx.TargetCharacter.GetComponent<RagdollMap>();
                        if (ragdollMap != null)
                        {
                            Undo.RecordObject(ragdollMap, "Auto Calculate Chain Mass");
                        }
                    }

                    chain.DistributeMassAcrossNodes();

                    Debug.Log($"Mass distributed across {chain.Nodes.Count} nodes in chain '{chain.ChainName}'");

                    SceneView.RepaintAll();
                }
            );

            menu.AddSeparator("");
            menu.AddDisabledItem(new GUIContent($"Current Total Mass: {chain.TotalMass:F2}"));
            menu.AddDisabledItem(new GUIContent($"Nodes in Chain: {chain.Nodes.Count}"));

            menu.ShowAsContext();
        }

        #endregion
    }
}
#endif
