#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;

namespace PhoenixRealm.RagdollCreatorPro.Editor
{
    internal class MassOverlayUI : IOverlayTabUI
    {
        #region Vars + Properties

        private FloatField m_totalMassField;
        private FloatField m_nodeMassField;
        private Button m_autoCalcBtn;
        private Button m_distributeBtn;
        private ScrollView m_nodeListScrollView;
        private VisualElement m_nodeListContainer;
        private Label m_massStatsLabel;

        // State tracking
        private CustomChain m_currentChain;
        private CustomNode m_currentNode;
        private RagdollMakerContext m_currentContext;
        private int m_selectedNodeIndex = -1;
        private bool m_isUpdating = false;
        private bool m_isInitialized = false;
        private bool m_callbacksRegistered = false;

        // Node UI tracking
        private readonly Dictionary<CustomNode, VisualElement> m_nodeUIElements = new Dictionary<CustomNode, VisualElement>();

        public bool IsInitialized => m_isInitialized;

        #endregion

        #region IOverlayTabUI Implementation

        public void Initialize(VisualElement tabPanel)
        {
            if (tabPanel == null)
            {
                Debug.LogError("[MassOverlayUI] Initialize: tabPanel is null!");
                return;
            }

            // Get UI elements
            m_totalMassField = tabPanel.Q<FloatField>("total-mass-field");
            m_nodeMassField = tabPanel.Q<FloatField>("node-mass-field");
            m_autoCalcBtn = tabPanel.Q<Button>("auto-calc-btn");
            m_distributeBtn = tabPanel.Q<Button>("distribute-btn");
            m_nodeListScrollView = tabPanel.Q<ScrollView>("node-list-scrollview");
            m_nodeListContainer = tabPanel.Q<VisualElement>("node-list-container");
            m_massStatsLabel = tabPanel.Q<Label>("mass-stats-label");

            // Fallback to alternative container if not found
            if (m_nodeListContainer == null && m_nodeListScrollView != null)
            {
                m_nodeListContainer = m_nodeListScrollView.contentContainer;
            }

            m_isInitialized = (m_totalMassField != null && m_nodeMassField != null && m_nodeListContainer != null);

            if (m_isInitialized)
            {
                RegisterBaseCallbacks();
                UpdateMassStatsLabel("No chain selected");
            }
        }
        public void UpdateContent(VisualElement tabPanel, RagdollMakerContext ctx, CustomNode node)
        {
            if (!m_isInitialized || m_totalMassField == null)
            {
                Initialize(tabPanel);
                if (!m_isInitialized || m_totalMassField == null)
                {
                    Debug.LogError("[MassOverlayUI] UpdateContent: Failed to initialize UI elements. Cannot update content.");
                    return;
                }
            }

            if (ctx == null)
            {
                UpdateMassStatsLabel("No context available");
                return;
            }

            // Unsubscribe from previous context if any
            if (m_currentContext != null && m_currentContext != ctx)
            {
                UnsubscribeFromNotifications(m_currentContext);
            }

            m_isUpdating = true;
            m_callbacksRegistered = false;
            m_currentContext = ctx;
            m_currentNode = node;

            // Subscribe to notifications from the new context
            SubscribeToNotifications(ctx);

            // Get current chain
            CustomChain chain = null;
            if (ctx.SelectedChain >= 0 && ctx.SelectedChain < ctx.Chains.Count)
            {
                chain = ctx.Chains[ctx.SelectedChain];
                m_selectedNodeIndex = ctx.SelectedNode;
            }

            m_currentChain = chain;

            if (chain == null)
            {
                UpdateMassStatsLabel("No chain selected");
                ClearUI();
                m_isUpdating = false;
                return;
            }

            // Update chain-level mass controls
            UpdateChainMassControls(chain);

            // Update node-level mass controls
            UpdateNodeMassControls(node);

            // Update node list
            UpdateNodeList(chain);

            // Update statistics
            UpdateMassStatsLabel($"Chain: {chain.ChainName} ({chain.NodeCount} nodes)");

            m_isUpdating = false;
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
            m_isUpdating = false;
            m_currentChain = null;
            m_currentNode = null;
            m_currentContext = null;
            m_selectedNodeIndex = -1;
            m_nodeUIElements.Clear();
        }

        #endregion

        #region Notification System
        private void SubscribeToNotifications(RagdollMakerContext ctx)
        {
            if (ctx != null)
            {
                ctx.OnNodeModified -= OnNodeModifiedExternal;
                ctx.OnNodeModified += OnNodeModifiedExternal;

                ctx.OnChainModified -= OnChainModifiedExternal;
                ctx.OnChainModified += OnChainModifiedExternal;
            }
        }

        private void UnsubscribeFromNotifications(RagdollMakerContext ctx)
        {
            if (ctx != null)
            {
                ctx.OnNodeModified -= OnNodeModifiedExternal;
                ctx.OnChainModified -= OnChainModifiedExternal;
            }
        }

        /// <summary>Called when a node is modified externally (e.g., via scene handles)</summary>
        private void OnNodeModifiedExternal(CustomNode modifiedNode)
        {
            // Refresh the node's mass display if it's in our current chain
            if (m_currentChain != null && m_currentChain.Nodes.Contains(modifiedNode))
            {
                // Update the node mass field if this is the selected node
                if (modifiedNode == m_currentNode && m_nodeMassField != null)
                {
                    m_nodeMassField.SetValueWithoutNotify(modifiedNode.MassOverride);
                }

                // Update the node's mass display in the list
                UpdateNodeMassDisplay(modifiedNode, modifiedNode.MassOverride);

                // Update overall chain stats
                UpdateCurrentChainMassStats();
            }
        }

        /// <summary>Called when a chain is modified externally</summary>
        private void OnChainModifiedExternal(CustomChain modifiedChain)
        {
            if (modifiedChain == m_currentChain)
            {
                // Refresh the entire chain UI
                if (m_totalMassField != null)
                {
                    m_totalMassField.SetValueWithoutNotify(modifiedChain.TotalMass);
                }

                UpdateNodeList(modifiedChain);
                UpdateCurrentChainMassStats();
            }
        }

        #endregion

        #region Private Methods

        private void RegisterBaseCallbacks()
        {
            if (m_autoCalcBtn != null)
            {
                m_autoCalcBtn.clicked += OnAutoCalcClicked;
            }

            if (m_distributeBtn != null)
            {
                m_distributeBtn.clicked += OnDistributeClicked;
            }
        }

        private void UpdateChainMassControls(CustomChain chain)
        {
            if (m_totalMassField != null)
            {
                m_totalMassField.SetValueWithoutNotify(chain.TotalMass);

                // Register callback for total mass changes
                if (!m_callbacksRegistered)
                {
                    m_totalMassField.RegisterValueChangedCallback(evt =>
                    {
                        if (!m_isUpdating && m_currentChain == chain && m_currentContext != null)
                        {
                            Undo.RecordObject(m_currentContext, "Edit Chain Total Mass");
                            chain.TotalMass = Mathf.Max(0.1f, evt.newValue);
                            UpdateMassStatsLabel($"Total mass: {chain.TotalMass:F2} kg");
                        }
                    });
                }
            }
        }

        private void UpdateNodeMassControls(CustomNode node)
        {
            if (m_nodeMassField != null)
            {
                if (node != null)
                {
                    m_nodeMassField.SetValueWithoutNotify(node.MassOverride);
                    m_nodeMassField.SetEnabled(true);

                    // Register callback for node mass changes
                    if (!m_callbacksRegistered)
                    {
                        m_nodeMassField.RegisterValueChangedCallback(evt =>
                        {
                            if (!m_isUpdating && m_currentNode == node && m_currentContext != null)
                            {
                                Undo.RecordObject(m_currentContext, "Edit Node Mass");
                                node.MassOverride = Mathf.Max(0.0001f, evt.newValue);

                                // Update node list display
                                UpdateNodeMassDisplay(node, node.MassOverride);
                                UpdateCurrentChainMassStats();
                            }
                        });
                    }
                }
                else
                {
                    m_nodeMassField.SetValueWithoutNotify(0f);
                    m_nodeMassField.SetEnabled(false);
                }
            }
        }

        private void UpdateNodeList(CustomChain chain)
        {
            if (m_nodeListContainer == null) return;

            m_nodeListContainer.Clear();
            m_nodeUIElements.Clear();

            if (chain?.Nodes == null) return;

            for (int i = 0; i < chain.Nodes.Count; i++)
            {
                var node = chain.Nodes[i];
                if (node == null) continue;

                var nodeElement = CreateNodeListItem(node, i);
                m_nodeListContainer.Add(nodeElement);
                m_nodeUIElements[node] = nodeElement;
            }

            UpdateCurrentChainMassStats();
        }

        private VisualElement CreateNodeListItem(CustomNode node, int index)
        {
            var container = new VisualElement();
            container.AddToClassList("node-list-item");

            if (index == m_selectedNodeIndex)
            {
                container.AddToClassList("node-list-item--selected");
            }

            // Node name label
            var nameLabel = new Label(node.Transform?.name ?? $"Node {index}");
            nameLabel.AddToClassList("node-name-label");
            container.Add(nameLabel);

            // Mass field
            var massField = new FloatField();
            massField.SetValueWithoutNotify(node.MassOverride);
            massField.AddToClassList("node-mass-field");

            massField.RegisterValueChangedCallback(evt =>
            {
                if (!m_isUpdating && m_currentContext != null)
                {
                    Undo.RecordObject(m_currentContext, "Edit Node Mass in List");
                    node.MassOverride = Mathf.Max(0.0001f, evt.newValue);

                    // Update main mass field if this is the selected node
                    if (node == m_currentNode && m_nodeMassField != null)
                    {
                        m_nodeMassField.SetValueWithoutNotify(node.MassOverride);
                    }

                    UpdateCurrentChainMassStats();
                }
            });

            container.Add(massField);

            // Click to select
            container.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (m_currentContext != null && m_currentChain != null)
                {
                    var nodeIndex = m_currentChain.Nodes.IndexOf(node);
                    if (nodeIndex >= 0)
                    {
                        m_currentContext.SelectedNode = nodeIndex;

                        // Update selection visuals
                        UpdateNodeListSelection(nodeIndex);

                        // Repaint scene view to show new selection
                        EditorApplication.delayCall += () => SceneView.RepaintAll();
                    }
                }
            });

            return container;
        }

        private void UpdateNodeListSelection(int selectedIndex)
        {
            if (m_nodeListContainer == null) return;

            var nodeItems = m_nodeListContainer.Children();
            int index = 0;

            foreach (var item in nodeItems)
            {
                item.RemoveFromClassList("node-list-item--selected");
                if (index == selectedIndex)
                {
                    item.AddToClassList("node-list-item--selected");
                }
                index++;
            }

            m_selectedNodeIndex = selectedIndex;
        }

        private void UpdateNodeMassDisplay(CustomNode node, float newMass)
        {
            if (m_nodeUIElements.TryGetValue(node, out var nodeElement))
            {
                var massField = nodeElement.Q<FloatField>();
                if (massField != null)
                {
                    massField.SetValueWithoutNotify(newMass);
                }
            }
        }

        private void UpdateCurrentChainMassStats()
        {
            if (m_currentChain == null)
            {
                UpdateMassStatsLabel("No chain selected");
                return;
            }

            float totalNodeMass = 0f;
            int nodeCount = 0;

            foreach (var node in m_currentChain.Nodes)
            {
                if (node != null)
                {
                    totalNodeMass += node.MassOverride;
                    nodeCount++;
                }
            }

            float avgMass = nodeCount > 0 ? totalNodeMass / nodeCount : 0f;
            UpdateMassStatsLabel($"Nodes: {nodeCount} | Sum: {totalNodeMass:F2} kg | Avg: {avgMass:F2} kg");
        }

        private void OnAutoCalcClicked()
        {
            if (m_currentChain == null || m_currentContext == null)
            {
                UpdateMassStatsLabel("Error: No chain selected");
                return;
            }

            Undo.RecordObject(m_currentContext, "Auto-Calculate Chain Mass");

            // Calculate total mass based on volume heuristics
            float estimatedMass = CalculateEstimatedChainMass(m_currentChain);

            if (estimatedMass > 0f)
            {
                m_currentChain.TotalMass = estimatedMass;

                if (m_totalMassField != null)
                {
                    m_totalMassField.SetValueWithoutNotify(estimatedMass);
                }

                UpdateMassStatsLabel($"Auto-calculated mass: {estimatedMass:F2} kg");
            }
            else
            {
                UpdateMassStatsLabel("Auto-calc failed: No valid colliders");
            }

            EditorUtility.SetDirty(m_currentContext);
        }
        private void OnDistributeClicked()
        {
            if (m_currentChain == null || m_currentContext == null)
            {
                UpdateMassStatsLabel("Error: No chain selected");
                return;
            }

            Debug.Log($"Before distribution - Chain has {m_currentChain.Nodes.Count} nodes");

            Undo.RecordObject(m_currentContext, "Distribute Chain Mass");

            m_currentChain.DistributeMassAcrossNodes();

            // Notify that the chain was modified
            m_currentContext.NotifyChainModified(m_currentChain);

            // Update UI
            if (m_currentNode != null && m_nodeMassField != null)
            {
                m_nodeMassField.SetValueWithoutNotify(m_currentNode.MassOverride);
            }

            // Update node list
            UpdateNodeList(m_currentChain);

            UpdateMassStatsLabel("Mass distributed across nodes");
            EditorUtility.SetDirty(m_currentContext);

            Debug.Log($"Mass distribution completed for chain '{m_currentChain.ChainName}'");
        }


        private float CalculateEstimatedChainMass(CustomChain chain)
        {
            if (chain?.Nodes == null) return 50f; // Default fallback

            float totalVolume = 0f;
            const float densityKgPerCubicMeter = 985f; // Human body density

            foreach (var node in chain.Nodes)
            {
                if (node?.Transform == null) continue;

                float volume = 0f;

                switch (node.ColliderType)
                {
                    case ColliderType.Sphere:
                        float radius = node.ColliderRadius;
                        volume = (4f / 3f) * Mathf.PI * radius * radius * radius;
                        break;

                    case ColliderType.Capsule:
                        float capRadius = node.ColliderRadius;
                        float capHeight = node.ColliderHeight;
                        volume = Mathf.PI * capRadius * capRadius * (capHeight + (4f / 3f) * capRadius);
                        break;

                    case ColliderType.Box:
                        var size = node.ColliderSize;
                        volume = size.x * size.y * size.z;
                        break;
                }

                totalVolume += volume;
            }

            if (totalVolume <= 0f) return 50f; // Default fallback

            float massKg = totalVolume * densityKgPerCubicMeter;
            return Mathf.Clamp(massKg, 1f, 200f); // Reasonable range for human-scale ragdolls
        }

        private void ClearUI()
        {
            if (m_totalMassField != null)
            {
                m_totalMassField.SetValueWithoutNotify(0f);
            }

            if (m_nodeMassField != null)
            {
                m_nodeMassField.SetValueWithoutNotify(0f);
                m_nodeMassField.SetEnabled(false);
            }

            if (m_nodeListContainer != null)
            {
                m_nodeListContainer.Clear();
            }

            m_nodeUIElements.Clear();
        }

        private void UpdateMassStatsLabel(string message)
        {
            if (m_massStatsLabel != null)
            {
                m_massStatsLabel.text = message;
            }
        }

        #endregion
    }
}
#endif
