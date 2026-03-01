#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace PhoenixRealm.RagdollCreatorPro.Editor
{
    internal class ColliderOverlayUI : IOverlayTabUI
    {
        #region Vars + Properties

        private Vector3Field m_localOffsetField;
        private EnumField m_colliderTypeField;

        private FloatField m_sphereRadiusField;

        private FloatField m_capsuleRadiusField;
        private FloatField m_capsuleHeightField;
        private EnumField m_capsuleDirectionField;
        private Label m_capsuleTotalHeightLabel;

        private Vector3Field m_boxSizeField;

        private CustomNode m_currentNode;
        private RagdollMakerContext m_currentContext;
        private bool m_isInitialized = false;
        private bool m_callbacksRegistered = false;
        private bool m_isUpdating = false;

        public bool IsInitialized => m_isInitialized;

        #endregion

        #region IOverlayTabUI Implementation

        public void Initialize(VisualElement tabPanel)
        {
            if (tabPanel == null)
            {
                Debug.LogError("[ColliderOverlayUI] Initialize: tabPanel is null!");
                return;
            }

            m_localOffsetField = tabPanel.Q<Vector3Field>("local-offset-field");
            m_colliderTypeField = tabPanel.Q<EnumField>("collider-type-field");

            m_sphereRadiusField = tabPanel.Q<FloatField>("sphere-radius-field");

            m_capsuleRadiusField = tabPanel.Q<FloatField>("capsule-radius-field");
            m_capsuleHeightField = tabPanel.Q<FloatField>("capsule-height-field");
            m_capsuleDirectionField = tabPanel.Q<EnumField>("capsule-direction-field");
            m_capsuleTotalHeightLabel = tabPanel.Q<Label>("capsule-total-height-label");

            m_boxSizeField = tabPanel.Q<Vector3Field>("box-size-field");

            if (m_colliderTypeField != null)
            {
                m_colliderTypeField.Init(ColliderType.Capsule);
            }

            if (m_capsuleDirectionField != null)
            {
                m_capsuleDirectionField.Init(CapsuleDirection.Y);
            }

            m_isInitialized = (m_localOffsetField != null && m_colliderTypeField != null);
        }

        public void UpdateContent(VisualElement tabPanel, RagdollMakerContext ctx, CustomNode node)
        {
            if (!m_isInitialized || m_localOffsetField == null)
            {
                Initialize(tabPanel);
                if (!m_isInitialized || m_localOffsetField == null)
                {
                    Debug.LogError("[ColliderOverlayUI] UpdateContent: Failed to initialize UI elements. Cannot update content.");
                    return;
                }
            }

            if (node == null || ctx == null)
            {
                Debug.LogError("[ColliderOverlayUI] UpdateContent: Node or context is null");
                return;
            }

            if (m_currentContext != null && m_currentContext != ctx)
            {
                UnsubscribeFromNotifications(m_currentContext);
            }

            m_callbacksRegistered = false;
            m_currentNode = node;
            m_currentContext = ctx;

            SubscribeToNotifications(ctx);

            RefreshUIFromNode(node, tabPanel);

            m_callbacksRegistered = true;
        }

        public void Cleanup()
        {
            if (m_currentContext != null)
            {
                UnsubscribeFromNotifications(m_currentContext);
            }

            m_callbacksRegistered = false;
            m_isInitialized = false;
            m_currentNode = null;
            m_currentContext = null;
        }

        #endregion

        #region Notification System

        private void SubscribeToNotifications(RagdollMakerContext ctx)
        {
            if (ctx != null)
            {
                ctx.OnNodeModified -= OnNodeModifiedExternal;
                ctx.OnNodeModified += OnNodeModifiedExternal;
            }
        }

        private void UnsubscribeFromNotifications(RagdollMakerContext ctx)
        {
            if (ctx != null)
            {
                ctx.OnNodeModified -= OnNodeModifiedExternal;
            }
        }

        private void OnNodeModifiedExternal(CustomNode modifiedNode)
        {
            if (modifiedNode == m_currentNode && m_currentContext != null)
            {
                VisualElement tabPanel = FindTabPanel();
                if (tabPanel != null)
                {
                    RefreshUIFromNode(modifiedNode, tabPanel);
                }
            }
        }

        private VisualElement FindTabPanel()
        {
            if (m_localOffsetField != null)
            {
                var parent = m_localOffsetField.parent;
                while (parent != null && !parent.ClassListContains("panel"))
                {
                    parent = parent.parent;
                }
                return parent;
            }
            return null;
        }

        private void RefreshUIFromNode(CustomNode node, VisualElement tabPanel)
        {
            if (node == null) return;

            m_isUpdating = true;

            if (m_localOffsetField != null)
                m_localOffsetField.SetValueWithoutNotify(node.LocalOffset);

            if (m_colliderTypeField != null)
                m_colliderTypeField.SetValueWithoutNotify(node.ColliderType);

            if (tabPanel != null)
            {
                UpdateColliderParameterVisibility(tabPanel, node);
                UpdateCommonCallbacks(m_currentContext, node);
                UpdateColliderParameterValues(m_currentContext, node);
            }

            m_isUpdating = false;
        }

        #endregion

        #region Private Methods

        private void UpdateCommonCallbacks(RagdollMakerContext ctx, CustomNode node)
        {
            if (!m_callbacksRegistered)
            {
                if (m_localOffsetField != null)
                {
                    m_localOffsetField.RegisterValueChangedCallback(evt =>
                    {
                        if (!m_isUpdating && m_currentNode == node && m_currentContext == ctx)
                        {
                            Undo.RecordObject(ctx, "Edit Node Local Offset");
                            node.LocalOffset = evt.newValue;
                        }
                    });
                }

                if (m_colliderTypeField != null)
                {
                    m_colliderTypeField.RegisterValueChangedCallback(evt =>
                    {
                        if (!m_isUpdating && m_currentNode == node && m_currentContext == ctx)
                        {
                            Undo.RecordObject(ctx, "Change Collider Type");
                            node.ColliderType = (ColliderType)evt.newValue;

                            var colliderPanel = m_colliderTypeField?.parent;
                            while (colliderPanel != null && !colliderPanel.ClassListContains("panel"))
                            {
                                colliderPanel = colliderPanel.parent;
                            }

                            if (colliderPanel != null)
                            {
                                UpdateColliderParameterVisibility(colliderPanel, node);
                                UpdateColliderParameterValues(ctx, node);
                            }
                        }
                    });
                }
            }
        }

        private void UpdateColliderParameterVisibility(VisualElement colliderPanel, CustomNode node)
        {
            colliderPanel.RemoveFromClassList("show-sphere");
            colliderPanel.RemoveFromClassList("show-capsule");
            colliderPanel.RemoveFromClassList("show-box");

            switch (node.ColliderType)
            {
                case ColliderType.Sphere:
                    colliderPanel.AddToClassList("show-sphere");
                    break;
                case ColliderType.Capsule:
                    colliderPanel.AddToClassList("show-capsule");
                    break;
                case ColliderType.Box:
                    colliderPanel.AddToClassList("show-box");
                    break;
            }
        }

        private void UpdateColliderParameterValues(RagdollMakerContext ctx, CustomNode node)
        {
            switch (node.ColliderType)
            {
                case ColliderType.Sphere:
                    UpdateSphereParameters(ctx, node);
                    break;
                case ColliderType.Capsule:
                    UpdateCapsuleParameters(ctx, node);
                    break;
                case ColliderType.Box:
                    UpdateBoxParameters(ctx, node);
                    break;
            }
        }

        private void UpdateSphereParameters(RagdollMakerContext ctx, CustomNode node)
        {
            if (m_sphereRadiusField == null) return;

            m_sphereRadiusField.SetValueWithoutNotify(node.ColliderRadius);

            if (!m_callbacksRegistered)
            {
                m_sphereRadiusField.RegisterValueChangedCallback(evt =>
                {
                    if (!m_isUpdating && m_currentNode == node && m_currentContext == ctx)
                    {
                        Undo.RecordObject(ctx, "Edit Sphere Radius");
                        node.ColliderRadius = Mathf.Max(0f, evt.newValue);
                    }
                });
            }
        }

        private void UpdateCapsuleParameters(RagdollMakerContext ctx, CustomNode node)
        {
            if (m_capsuleRadiusField == null || m_capsuleHeightField == null) return;

            m_capsuleRadiusField.SetValueWithoutNotify(node.ColliderRadius);
            m_capsuleHeightField.SetValueWithoutNotify(node.ColliderHeight);

            if (m_capsuleDirectionField != null)
            {
                m_capsuleDirectionField.SetValueWithoutNotify(node.CapsuleDirection);
            }

            UpdateCapsuleTotalHeight(node);

            if (!m_callbacksRegistered)
            {
                m_capsuleRadiusField.RegisterValueChangedCallback(evt =>
                {
                    if (!m_isUpdating && m_currentNode == node && m_currentContext == ctx)
                    {
                        Undo.RecordObject(ctx, "Edit Capsule Radius");
                        node.ColliderRadius = Mathf.Max(0f, evt.newValue);
                        UpdateCapsuleTotalHeight(node);
                    }
                });

                m_capsuleHeightField.RegisterValueChangedCallback(evt =>
                {
                    if (!m_isUpdating && m_currentNode == node && m_currentContext == ctx)
                    {
                        Undo.RecordObject(ctx, "Edit Capsule Height");
                        node.ColliderHeight = Mathf.Max(0f, evt.newValue);
                        UpdateCapsuleTotalHeight(node);
                    }
                });

                if (m_capsuleDirectionField != null)
                {
                    m_capsuleDirectionField.RegisterValueChangedCallback(evt =>
                    {
                        if (!m_isUpdating && m_currentNode == node && m_currentContext == ctx)
                        {
                            Undo.RecordObject(ctx, "Change Capsule Direction");
                            node.CapsuleDirection = (CapsuleDirection)evt.newValue;
                        }
                    });
                }
            }
        }

        private void UpdateCapsuleTotalHeight(CustomNode node)
        {
            if (m_capsuleTotalHeightLabel != null)
            {
                var totalHeight = node.ColliderHeight + node.ColliderRadius * 2f;
                m_capsuleTotalHeightLabel.text = $"Total Height: {totalHeight:F3}";
            }
        }

        private void UpdateBoxParameters(RagdollMakerContext ctx, CustomNode node)
        {
            if (m_boxSizeField == null) return;

            m_boxSizeField.SetValueWithoutNotify(node.ColliderSize);

            if (!m_callbacksRegistered)
            {
                m_boxSizeField.RegisterValueChangedCallback(evt =>
                {
                    if (!m_isUpdating && m_currentNode == node && m_currentContext == ctx)
                    {
                        Undo.RecordObject(ctx, "Edit Box Size");
                        var newSize = evt.newValue;
                        newSize.x = Mathf.Max(0f, newSize.x);
                        newSize.y = Mathf.Max(0f, newSize.y);
                        newSize.z = Mathf.Max(0f, newSize.z);
                        node.ColliderSize = newSize;
                    }
                });
            }
        }

        #endregion
    }
}
#endif
