#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;
using UnityEditor;

namespace PhoenixRealm.RagdollCreatorPro.Editor
{
    public class RagdollChainManager
    {
        #region Vars + Properties

        private RagdollMakerContext m_ctx;
        private readonly Dictionary<CustomChain, bool> m_chainExpandedStates = new Dictionary<CustomChain, bool>();

        public System.Action OnChainsChanged;

        #endregion

        #region Constructor

        public RagdollChainManager(RagdollMakerContext ctx)
        {
            m_ctx = ctx;
        }

        #endregion

        #region Public API

        public void AddNewChain()
        {
            var newChain = new CustomChain($"Chain {m_ctx.Chains.Count + 1}");
            m_ctx.Chains.Add(newChain);
            m_chainExpandedStates[newChain] = true;
            OnChainsChanged?.Invoke();
        }

        public void DuplicateChain(int chainIndex)
        {
            if (chainIndex < 0 || chainIndex >= m_ctx.Chains.Count)
            {
                Debug.LogWarning($"[RagdollChainManager] Cannot duplicate chain at invalid index {chainIndex}");
                return;
            }

            Undo.RecordObject(m_ctx, "Duplicate Chain");

            var originalChain = m_ctx.Chains[chainIndex];
            var duplicatedChain = originalChain.Clone();

            m_ctx.Chains.Insert(chainIndex + 1, duplicatedChain);
            m_chainExpandedStates[duplicatedChain] = true;

            OnChainsChanged?.Invoke();

            Debug.Log($"[RagdollChainManager] Duplicated chain '{originalChain.ChainName}' to '{duplicatedChain.ChainName}'");
        }

        public void BuildChainsUI(VisualElement chainsContainer)
        {
            if (m_ctx?.Chains == null) return;

            for (int i = 0; i < m_ctx.Chains.Count; i++)
            {
                var chain = m_ctx.Chains[i];
                var chainElement = CreateChainElement(chain, i);
                chainsContainer.Add(chainElement);
            }
        }

        #endregion

        #region Chain UI Creation

        private VisualElement CreateChainElement(CustomChain chain, int chainIndex)
        {
            var chainCard = new VisualElement();
            chainCard.AddToClassList("chain-card");

            var chainHeader = CreateChainHeader(chain, chainIndex);
            var chainContent = CreateChainContent(chain);

            SetupChainExpansion(chain, chainCard, chainHeader, chainContent);

            chainCard.Add(chainHeader);
            chainCard.Add(chainContent);

            return chainCard;
        }

        private VisualElement CreateChainHeader(CustomChain chain, int chainIndex)
        {
            var chainHeader = new VisualElement();
            chainHeader.AddToClassList("chain-header");

            var titleContainer = CreateChainTitleContainer(chain);
            var actions = CreateChainActions(chainIndex);

            chainHeader.Add(titleContainer);
            chainHeader.Add(actions);

            return chainHeader;
        }

        private VisualElement CreateChainTitleContainer(CustomChain chain)
        {
            var titleContainer = new VisualElement();
            titleContainer.AddToClassList("chain-title-container");

            var expandToggle = CreateChainExpandToggle(chain);
            var nameField = CreateChainNameField(chain);

            titleContainer.Add(expandToggle);
            titleContainer.Add(nameField);

            return titleContainer;
        }

        private Button CreateChainExpandToggle(CustomChain chain)
        {
            var expandToggle = new Button();
            expandToggle.AddToClassList("chain-expand-toggle");

            bool isExpanded = m_chainExpandedStates.GetValueOrDefault(chain, false);
            expandToggle.text = isExpanded ? "−" : "+";

            return expandToggle;
        }

        private TextField CreateChainNameField(CustomChain chain)
        {
            var nameField = new TextField();
            nameField.AddToClassList("chain-name-field");
            nameField.SetValueWithoutNotify(chain.ChainName);
            nameField.RegisterValueChangedCallback(evt =>
            {
                chain.ChainName = evt.newValue;
            });

            return nameField;
        }

        private VisualElement CreateChainActions(int chainIndex)
        {
            var actions = new VisualElement();
            actions.AddToClassList("chain-actions");

            var orderControls = CreateChainOrderControls(chainIndex);
            var duplicateBtn = CreateChainDuplicateButton(chainIndex);
            var removeBtn = CreateChainRemoveButton(chainIndex);

            actions.Add(orderControls);
            actions.Add(duplicateBtn);
            actions.Add(removeBtn);

            return actions;
        }

        private VisualElement CreateChainOrderControls(int chainIndex)
        {
            var orderContainer = new VisualElement();
            orderContainer.AddToClassList("chain-order-controls");

            var moveUpBtn = CreateMoveButton("▲", () => MoveChainUp(chainIndex));
            var moveDownBtn = CreateMoveButton("▼", () => MoveChainDown(chainIndex));

            orderContainer.Add(moveUpBtn);
            orderContainer.Add(moveDownBtn);

            return orderContainer;
        }

        private Button CreateMoveButton(string text, System.Action onClick)
        {
            var button = new Button();
            button.AddToClassList("chain-move-btn");
            button.text = text;
            button.clicked += onClick;
            return button;
        }

        private Button CreateChainDuplicateButton(int chainIndex)
        {
            var duplicateBtn = new Button();
            duplicateBtn.AddToClassList("chain-duplicate-btn");
            duplicateBtn.text = "D";
            duplicateBtn.clicked += () => DuplicateChain(chainIndex);
            return duplicateBtn;
        }

        private Button CreateChainRemoveButton(int chainIndex)
        {
            var removeBtn = new Button();
            removeBtn.AddToClassList("chain-remove-btn");
            removeBtn.text = "×";
            removeBtn.clicked += () => RemoveChainAtIndex(chainIndex);
            return removeBtn;
        }

        private VisualElement CreateChainContent(CustomChain chain)
        {
            var chainContent = new VisualElement();
            chainContent.AddToClassList("chain-content");

            var parentNodeContainer = CreateParentNodeContainer(chain);
            var nodesContainer = CreateNodesContainer(chain);

            chainContent.Add(parentNodeContainer);
            chainContent.Add(nodesContainer);

            return chainContent;
        }

        private VisualElement CreateParentNodeContainer(CustomChain chain)
        {
            var container = new VisualElement();
            container.AddToClassList("parent-node-container");

            var label = new Label("Parent Node:");
            label.AddToClassList("parent-node-label");

            var field = new ObjectField()
            {
                objectType = typeof(Transform),
                allowSceneObjects = true
            };
            field.AddToClassList("parent-node-field");
            field.SetValueWithoutNotify(chain.ParentNode?.Transform);

            field.RegisterValueChangedCallback(evt =>
            {
                var newTransform = evt.newValue as Transform;
                chain.ParentNode = newTransform != null ? new CustomNode(newTransform) : null;
                OnChainsChanged?.Invoke();
            });

            var row = new VisualElement();
            row.AddToClassList("parent-node-row");
            row.Add(label);
            row.Add(field);

            container.Add(row);

            var helpText = new Label("Optional: Connect this chain's first node to a node from another chain");
            helpText.AddToClassList("parent-node-help");
            container.Add(helpText);

            return container;
        }

        private VisualElement CreateNodesContainer(CustomChain chain)
        {
            var nodesContainer = new VisualElement();
            nodesContainer.AddToClassList("nodes-container");

            for (int i = 0; i < chain.Nodes.Count; i++)
            {
                var nodeElement = CreateNodeElement(chain, chain.Nodes[i], i);
                nodesContainer.Add(nodeElement);
            }

            var buttonsContainer = CreateNodeButtonsContainer(chain, nodesContainer);
            nodesContainer.Add(buttonsContainer);

            return nodesContainer;
        }

        private VisualElement CreateNodeElement(CustomChain parentChain, CustomNode node, int nodeIndex)
        {
            var nodeRow = new VisualElement();
            nodeRow.AddToClassList("node-row");

            var nodeField = CreateNodeField(parentChain, node, nodeIndex);
            var nodeControls = CreateNodeControls(parentChain, nodeIndex);

            nodeRow.Add(nodeField);
            nodeRow.Add(nodeControls);

            return nodeRow;
        }

        private ObjectField CreateNodeField(CustomChain parentChain, CustomNode node, int nodeIndex)
        {
            var nodeField = new ObjectField();
            nodeField.AddToClassList("node-field");
            nodeField.objectType = typeof(Transform);
            nodeField.label = $"Bone {nodeIndex}";
            nodeField.SetValueWithoutNotify(node.Transform);

            nodeField.RegisterValueChangedCallback(evt =>
            {
                var newTransform = evt.newValue as Transform;
                node.Transform = newTransform;
                node.NodeName = newTransform ? newTransform.name : $"Node {nodeIndex}";

                if (nodeIndex == 0 && newTransform != null)
                {
                    parentChain.ChainName = newTransform.name + " Chain";
                    OnChainsChanged?.Invoke();
                }
            });

            return nodeField;
        }

        private VisualElement CreateNodeControls(CustomChain parentChain, int nodeIndex)
        {
            var controls = new VisualElement();
            controls.AddToClassList("node-controls");

            var moveUpBtn = CreateMoveButton("▲", () => MoveNodeUp(parentChain, nodeIndex));
            var moveDownBtn = CreateMoveButton("▼", () => MoveNodeDown(parentChain, nodeIndex));
            var removeBtn = CreateNodeRemoveButton(parentChain, nodeIndex);

            controls.Add(moveUpBtn);
            controls.Add(moveDownBtn);
            controls.Add(removeBtn);

            return controls;
        }

        private Button CreateNodeRemoveButton(CustomChain parentChain, int nodeIndex)
        {
            var removeBtn = new Button();
            removeBtn.AddToClassList("node-remove-btn");
            removeBtn.text = "×";
            removeBtn.clicked += () =>
            {
                parentChain.RemoveNode(nodeIndex);
                OnChainsChanged?.Invoke();
            };
            return removeBtn;
        }

        private VisualElement CreateNodeButtonsContainer(CustomChain chain, VisualElement nodesContainer)
        {
            var buttonsContainer = new VisualElement();
            buttonsContainer.AddToClassList("node-buttons-container");

            var addNodeBtn = CreateAddNodeButton(chain, nodesContainer);
            var autoAddNodeBtn = CreateAutoAddNodeButton(chain);

            buttonsContainer.Add(addNodeBtn);
            buttonsContainer.Add(autoAddNodeBtn);

            return buttonsContainer;
        }

        public VisualElement CreatePresetButtons(RagdollMakerWindow window)
        {
            var presetContainer = new VisualElement();
            presetContainer.AddToClassList("preset-buttons-container");

            var savePresetBtn = new Button();
            savePresetBtn.AddToClassList("save-preset-btn");
            savePresetBtn.text = "Save As Preset";
            savePresetBtn.clicked += () => window.SaveCurrentAsPreset();

            var loadPresetBtn = new Button();
            loadPresetBtn.AddToClassList("load-preset-btn");
            loadPresetBtn.text = "Create From Preset";
            loadPresetBtn.clicked += () => ShowPresetSelectionMenu(window);

            presetContainer.Add(savePresetBtn);
            presetContainer.Add(loadPresetBtn);

            return presetContainer;
        }

        private void ShowPresetSelectionMenu(RagdollMakerWindow window)
        {
            var presets = RagdollPresetCreator.FindAllPresets();

            if (presets.Count == 0)
            {
                EditorUtility.DisplayDialog("No Presets Found",
                    "No ragdoll presets found in the project. Create one by clicking 'Save As Preset'.", "OK");
                return;
            }

            var menu = new GenericMenu();

            foreach (var preset in presets)
            {
                var presetName = preset.PresetName;
                if (!string.IsNullOrEmpty(preset.Description))
                {
                    presetName += $" ({preset.Description})";
                }

                menu.AddItem(new GUIContent(presetName), false, () =>
                {
                    window.CreateFromPreset(preset);
                });
            }

            menu.ShowAsContext();
        }


        private Button CreateAddNodeButton(CustomChain chain, VisualElement nodesContainer)
        {
            var addNodeBtn = new Button();
            addNodeBtn.AddToClassList("add-node-btn");
            addNodeBtn.text = "Add Node";
            addNodeBtn.clicked += () =>
            {
                chain.AddNode(null);
                OnChainsChanged?.Invoke();
            };
            return addNodeBtn;
        }

        private Button CreateAutoAddNodeButton(CustomChain chain)
        {
            var autoAddBtn = new Button();
            autoAddBtn.AddToClassList("auto-add-node-btn");
            autoAddBtn.text = "Auto Add Child";
            autoAddBtn.clicked += () => AutoAddChildNode(chain);
            return autoAddBtn;
        }

        #endregion

        #region Chain and Node Operations

        private void RemoveChainAtIndex(int chainIndex)
        {
            var chain = m_ctx.Chains[chainIndex];
            m_ctx.Chains.RemoveAt(chainIndex);

            if (m_chainExpandedStates.ContainsKey(chain))
            {
                m_chainExpandedStates.Remove(chain);
            }

            OnChainsChanged?.Invoke();
        }

        private void MoveChainUp(int chainIndex)
        {
            if (chainIndex > 0 && chainIndex < m_ctx.Chains.Count)
            {
                SwapChains(chainIndex, chainIndex - 1);
                OnChainsChanged?.Invoke();
            }
        }

        private void MoveChainDown(int chainIndex)
        {
            if (chainIndex >= 0 && chainIndex < m_ctx.Chains.Count - 1)
            {
                SwapChains(chainIndex, chainIndex + 1);
                OnChainsChanged?.Invoke();
            }
        }

        private void SwapChains(int fromIndex, int toIndex)
        {
            var temp = m_ctx.Chains[fromIndex];
            m_ctx.Chains[fromIndex] = m_ctx.Chains[toIndex];
            m_ctx.Chains[toIndex] = temp;
        }

        private void MoveNodeUp(CustomChain parentChain, int nodeIndex)
        {
            if (nodeIndex > 0 && nodeIndex < parentChain.Nodes.Count)
            {
                SwapNodes(parentChain, nodeIndex, nodeIndex - 1);
                OnChainsChanged?.Invoke();
            }
        }

        private void MoveNodeDown(CustomChain parentChain, int nodeIndex)
        {
            if (nodeIndex >= 0 && nodeIndex < parentChain.Nodes.Count - 1)
            {
                SwapNodes(parentChain, nodeIndex, nodeIndex + 1);
                OnChainsChanged?.Invoke();
            }
        }

        private void SwapNodes(CustomChain parentChain, int fromIndex, int toIndex)
        {
            var temp = parentChain.Nodes[fromIndex];
            parentChain.Nodes[fromIndex] = parentChain.Nodes[toIndex];
            parentChain.Nodes[toIndex] = temp;
        }

        private void AutoAddChildNode(CustomChain chain)
        {
            if (chain.Nodes.Count == 0) return;

            var lastNode = chain.Nodes[chain.Nodes.Count - 1];
            if (lastNode.Transform == null) return;

            if (lastNode.Transform.childCount > 0)
            {
                var childTransform = lastNode.Transform.GetChild(0);
                chain.AddNode(childTransform);
                OnChainsChanged?.Invoke();
            }
        }

        private void SetupChainExpansion(CustomChain chain, VisualElement chainCard, VisualElement chainHeader, VisualElement chainContent)
        {
            bool isExpanded = m_chainExpandedStates.GetValueOrDefault(chain, false);
            var expandToggle = chainHeader.Q<Button>("", "chain-expand-toggle");

            ApplyExpansionState(isExpanded, chainCard, chainHeader, chainContent);

            if (expandToggle != null)
            {
                expandToggle.clicked += () =>
                {
                    ToggleChainExpansion(chain, chainCard, chainHeader, chainContent, expandToggle);
                };
            }
        }

        private void ToggleChainExpansion(CustomChain chain, VisualElement chainCard, VisualElement chainHeader, VisualElement chainContent, Button expandToggle)
        {
            bool isExpanded = !m_chainExpandedStates.GetValueOrDefault(chain, false);
            m_chainExpandedStates[chain] = isExpanded;
            expandToggle.text = isExpanded ? "−" : "+";

            ApplyExpansionState(isExpanded, chainCard, chainHeader, chainContent);
        }

        private void ApplyExpansionState(bool isExpanded, VisualElement chainCard, VisualElement chainHeader, VisualElement chainContent)
        {
            if (isExpanded)
            {
                chainCard.AddToClassList("chain-card--expanded");
                chainHeader.AddToClassList("chain-header--expanded");
                chainContent.AddToClassList("chain-content--expanded");
            }
            else
            {
                chainCard.RemoveFromClassList("chain-card--expanded");
                chainHeader.RemoveFromClassList("chain-header--expanded");
                chainContent.RemoveFromClassList("chain-content--expanded");
            }
        }

        #endregion
    }
}
#endif
