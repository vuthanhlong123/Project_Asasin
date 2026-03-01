#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace PhoenixRealm.RagdollCreatorPro.Editor
{
    internal class FitOverlayUI : IOverlayTabUI
    {
        #region Vars + Properties

        private EnumField m_searchScopeField;
        private EnumField m_anchorModeField;
        private Button m_autoFitBtn;
        private Toggle m_fitPositionToggle;
        private Toggle m_fitSizeToggle;
        private Label m_statusLabel;

        // State
        private FitSearchScope m_SearchScope = FitSearchScope.BoneChildrenAndGrandchildren;
        private FitAnchorMode m_AnchorMode = FitAnchorMode.ForNonLimbs;
        private bool m_FitPosition = true;
        private bool m_FitSize = true;

        private System.Action m_currentClickHandler;
        private bool m_isInitialized = false;
        private bool m_callbacksRegistered = false;

        public bool IsInitialized => m_isInitialized;

        #endregion

        #region IOverlayTabUI Implementation

        public void Initialize(VisualElement tabPanel)
        {
            if (tabPanel == null)
            {
                Debug.LogError("[FitOverlayUI] Initialize: tabPanel is null!");
                return;
            }

            // Get UI elements
            m_searchScopeField = tabPanel.Q<EnumField>("search-scope-field");
            m_anchorModeField = tabPanel.Q<EnumField>("anchor-mode-field");
            m_autoFitBtn = tabPanel.Q<Button>("auto-fit-btn");
            m_fitPositionToggle = tabPanel.Q<Toggle>("fit-position-toggle");
            m_fitSizeToggle = tabPanel.Q<Toggle>("fit-size-toggle");
            m_statusLabel = tabPanel.Q<Label>("fit-status-label");

            // Initialize enum fields
            if (m_searchScopeField != null)
            {
                m_searchScopeField.Init(m_SearchScope);
            }

            if (m_anchorModeField != null)
            {
                m_anchorModeField.Init(m_AnchorMode);
            }

            // Set initial toggle values
            if (m_fitPositionToggle != null)
            {
                m_fitPositionToggle.SetValueWithoutNotify(m_FitPosition);
            }

            if (m_fitSizeToggle != null)
            {
                m_fitSizeToggle.SetValueWithoutNotify(m_FitSize);
            }

            // Register base callbacks for settings
            RegisterSettingsCallbacks();

            m_isInitialized = (m_searchScopeField != null && m_autoFitBtn != null);

            if (m_isInitialized)
            {
                UpdateStatusLabel("Ready to auto-fit collider");
            }
        }

        public void UpdateContent(VisualElement tabPanel, RagdollMakerContext ctx, CustomNode node)
        {
            if (!m_isInitialized || m_searchScopeField == null)
            {
                Initialize(tabPanel);
                if (!m_isInitialized || m_searchScopeField == null)
                {
                    Debug.LogError("[FitOverlayUI] UpdateContent: Failed to initialize UI elements. Cannot update content.");
                    return;
                }
            }

            if (node == null || ctx == null)
            {
                UpdateStatusLabel("No node selected");
                return;
            }

            // Clear existing auto-fit callback
            if (m_currentClickHandler != null && m_autoFitBtn != null)
            {
                m_autoFitBtn.clicked -= m_currentClickHandler;
                m_currentClickHandler = null;
            }

            // Update UI values
            if (m_searchScopeField != null)
            {
                m_searchScopeField.SetValueWithoutNotify(m_SearchScope);
            }

            if (m_anchorModeField != null)
            {
                m_anchorModeField.SetValueWithoutNotify(m_AnchorMode);
            }

            if (m_fitPositionToggle != null)
            {
                m_fitPositionToggle.SetValueWithoutNotify(m_FitPosition);
            }

            if (m_fitSizeToggle != null)
            {
                m_fitSizeToggle.SetValueWithoutNotify(m_FitSize);
            }

            // Register new auto-fit callback with current context and node
            m_currentClickHandler = () => OnAutoFitClicked(ctx, node);
            if (m_autoFitBtn != null)
            {
                m_autoFitBtn.clicked += m_currentClickHandler;
            }

            // Update status based on node state
            UpdateStatusForNode(node);
            m_callbacksRegistered = true;
        }

        public void Cleanup()
        {
            // Remove auto-fit callback
            if (m_currentClickHandler != null && m_autoFitBtn != null)
            {
                m_autoFitBtn.clicked -= m_currentClickHandler;
                m_currentClickHandler = null;
            }

            m_callbacksRegistered = false;
            m_isInitialized = false;

            // Reset to defaults
            m_SearchScope = FitSearchScope.BoneChildrenAndGrandchildren;
            m_AnchorMode = FitAnchorMode.ForNonLimbs;
            m_FitPosition = true;
            m_FitSize = true;
        }

        #endregion

        #region Private Methods

        private void RegisterSettingsCallbacks()
        {
            if (m_searchScopeField != null)
            {
                m_searchScopeField.RegisterValueChangedCallback(evt =>
                {
                    m_SearchScope = (FitSearchScope)evt.newValue;
                    UpdateStatusLabel($"Search scope: {m_SearchScope}");
                });
            }

            if (m_anchorModeField != null)
            {
                m_anchorModeField.RegisterValueChangedCallback(evt =>
                {
                    m_AnchorMode = (FitAnchorMode)evt.newValue;
                    UpdateStatusLabel($"Anchor mode: {m_AnchorMode}");
                });
            }

            if (m_fitPositionToggle != null)
            {
                m_fitPositionToggle.RegisterValueChangedCallback(evt =>
                {
                    m_FitPosition = evt.newValue;
                    UpdateStatusLabel($"Fit position: {(m_FitPosition ? "enabled" : "disabled")}");
                });
            }

            if (m_fitSizeToggle != null)
            {
                m_fitSizeToggle.RegisterValueChangedCallback(evt =>
                {
                    m_FitSize = evt.newValue;
                    UpdateStatusLabel($"Fit size: {(m_FitSize ? "enabled" : "disabled")}");
                });
            }
        }

        private void OnAutoFitClicked(RagdollMakerContext context, CustomNode node)
        {
            if (context == null || node == null || node.Transform == null)
            {
                UpdateStatusLabel("Error: Invalid node or context");
                Debug.LogWarning("[FitOverlayUI] Auto-Fit: No valid node selected.");
                return;
            }

            // Validate fit options
            if (!m_FitPosition && !m_FitSize)
            {
                UpdateStatusLabel("Error: Enable at least one fit option");
                return;
            }

            UpdateStatusLabel("Fitting collider...");

            try
            {
                Undo.RecordObject(context, "Auto-Fit Collider");

                bool success = ColliderAutoFitUtility.AutoFitNodeCollider(
                    node,
                    m_SearchScope,
                    m_AnchorMode,
                    m_FitPosition,
                    m_FitSize
                );

                if (success)
                {
                    UpdateStatusLabel("Auto-fit completed successfully");
                    EditorUtility.SetDirty(context);

                    // Trigger scene repaint
                    EditorApplication.delayCall += () => SceneView.RepaintAll();
                }
                else
                {
                    UpdateStatusLabel("Auto-fit failed - no suitable renderers found");
                }
            }
            catch (System.Exception ex)
            {
                UpdateStatusLabel($"Auto-fit error: {ex.Message}");
                Debug.LogError($"[FitOverlayUI] Auto-fit failed: {ex.Message}");
            }
        }

        private void UpdateStatusForNode(CustomNode node)
        {
            if (node?.Transform == null)
            {
                UpdateStatusLabel("No transform assigned to node");
                return;
            }

            // Count potential renderers in scope
            int rendererCount = CountRenderersInScope(node.Transform);

            if (rendererCount == 0)
            {
                UpdateStatusLabel("No renderers found in search scope");
            }
            else
            {
                UpdateStatusLabel($"Ready - {rendererCount} renderer(s) in scope");
            }
        }

        private int CountRenderersInScope(Transform transform)
        {
            if (transform == null) return 0;

            var renderers = new System.Collections.Generic.List<Renderer>();

            switch (m_SearchScope)
            {
                case FitSearchScope.BoneAndChildren:
                    CollectRenderersInChildren(transform, renderers, 1);
                    break;

                case FitSearchScope.BoneChildrenAndGrandchildren:
                    CollectRenderersInChildren(transform, renderers, 2);
                    break;

                case FitSearchScope.DeepHierarchy:
                    CollectRenderersInChildren(transform, renderers, int.MaxValue);
                    break;
            }

            return renderers.Count;
        }

        private void CollectRenderersInChildren(Transform parent, System.Collections.Generic.List<Renderer> renderers, int maxDepth)
        {
            if (parent == null || maxDepth <= 0) return;

            var renderer = parent.GetComponent<Renderer>();
            if (renderer != null && !(renderer is ParticleSystemRenderer))
            {
                renderers.Add(renderer);
            }

            if (maxDepth > 1)
            {
                for (int i = 0; i < parent.childCount; i++)
                {
                    CollectRenderersInChildren(parent.GetChild(i), renderers, maxDepth - 1);
                }
            }
        }

        private void UpdateStatusLabel(string message)
        {
            if (m_statusLabel != null)
            {
                m_statusLabel.text = message;
            }
        }

        #endregion
    }
}
#endif
