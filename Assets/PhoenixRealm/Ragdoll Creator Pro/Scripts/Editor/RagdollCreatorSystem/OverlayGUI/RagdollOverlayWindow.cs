#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using PhoenixRealm.RagdollCreatorPro;

namespace PhoenixRealm.RagdollCreatorPro.Editor
{
    public sealed class RagdollOverlayWindow
    {
        #region Vars
        private static readonly string[] s_ColliderTools = { "Move", "Rotate", "Scale" };
        private static readonly string[] s_JointTools = { "Anchor", "Axis", "Twist", "Swing" };

        private static readonly IOverlayTabUI[] s_TabUIHandlers = new IOverlayTabUI[]
        {
            new ColliderOverlayUI(),
            new JointOverlayUI(),
            new FitOverlayUI(),
            new MassOverlayUI()
        };

        private static int s_TopTabIndex = 0;

        // UI Toolkit elements
        private VisualElement m_overlayRoot;
        [SerializeField] private VisualTreeAsset m_overlayUXML;
        [SerializeField] private StyleSheet m_styleSheet;
        private bool m_isInitialized = false;

        // UI Element references
        private Label m_overlayTitle;
        private Button m_xrayToggle;
        private Button[] m_tabButtons = new Button[4];
        private VisualElement m_toolContainer;
        private VisualElement m_toolButtons;
        private VisualElement m_contentContainer;

        // Panel references
        private VisualElement[] m_tabPanels = new VisualElement[4];

        // Current context tracking
        private RagdollMakerContext m_currentContext;
        private CustomNode m_currentNode;
        private Transform m_currentBone;

        // Performance optimization
        private int m_lastSelectedChain = -1;
        private int m_lastSelectedNode = -1;
        private bool m_needsContentUpdate = true;
        private bool m_uiComponentsInitialized = false;

        #endregion

        #region GUI

        /// <summary>Main draw method for the overlay window</summary>
        public void Draw(RagdollMakerContext ctx)
        {
            if (ctx == null) return;

            if (m_overlayUXML == null)
            {
                Debug.LogWarning("[RagdollOverlayWindow] UXML not loaded, attempting to reload assets...");
                LoadAssets();

                if (m_overlayUXML == null)
                {
                    Debug.LogError("[RagdollOverlayWindow] Cannot load UXML - overlay disabled");
                    return;
                }
            }

            m_currentContext = ctx;

            bool selectionChanged = (ctx.SelectedChain != m_lastSelectedChain || ctx.SelectedNode != m_lastSelectedNode);
            if (selectionChanged)
            {
                m_lastSelectedChain = ctx.SelectedChain;
                m_lastSelectedNode = ctx.SelectedNode;
                m_needsContentUpdate = true;
                CleanupUIForSelectionChange();
            }

            if (EditorApplication.isPlaying)
            {
                DrawPlaymodeOverlay(ctx);
            }
            else
            {
                DrawEditModeOverlay(ctx);
            }
        }

        private void DrawPlaymodeOverlay(RagdollMakerContext ctx)
        {
            if (ctx.SelectedChain >= 0 && ctx.SelectedNode >= 0 &&
                ctx.SelectedChain < ctx.Chains.Count &&
                ctx.SelectedNode < ctx.Chains[ctx.SelectedChain].Nodes.Count)
            {
                var node = ctx.Chains[ctx.SelectedChain].Nodes[ctx.SelectedNode];
                if (node?.Transform != null)
                {
                    ShowPlaymodeInfo(ctx, node);
                }
            }
        }

        private void DrawEditModeOverlay(RagdollMakerContext ctx)
        {
            if (ctx.SelectedChain >= 0 && ctx.SelectedNode >= 0 &&
                ctx.SelectedChain < ctx.Chains.Count &&
                ctx.SelectedNode < ctx.Chains[ctx.SelectedChain].Nodes.Count)
            {
                var node = ctx.Chains[ctx.SelectedChain].Nodes[ctx.SelectedNode];
                if (node?.Transform != null)
                {
                    ShowOverlay(ctx, node, node.Transform);
                }
            }
        }

        private void ShowPlaymodeInfo(RagdollMakerContext ctx, CustomNode node)
        {
            Handles.BeginGUI();

            Vector3 worldPos = node.Transform.TransformPoint(node.LocalOffset);
            Vector2 screenPos = HandleUtility.WorldToGUIPoint(worldPos);

            var rect = new Rect(screenPos.x + 20, screenPos.y - 40, 250, 60);
            GUI.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
            GUI.Box(rect, "");

            GUI.color = Color.white;
            var labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontStyle = FontStyle.Bold;

            GUI.Label(new Rect(rect.x + 5, rect.y + 5, rect.width - 10, 18),
                $"Node: {node.Transform.name}", labelStyle);
            GUI.Label(new Rect(rect.x + 5, rect.y + 25, rect.width - 10, 15),
                $"Type: {node.ColliderType}");
            GUI.Label(new Rect(rect.x + 5, rect.y + 40, rect.width - 10, 15),
                "Playmode - Limited Editing");

            GUI.color = Color.white;
            Handles.EndGUI();
        }

        private void CleanupUIForSelectionChange()
        {
            if (m_uiComponentsInitialized)
            {
                foreach (var tabUI in s_TabUIHandlers)
                {
                    tabUI?.Cleanup();
                }
            }
        }

        /// <summary>Enhanced cleanup that preserves state during play mode transitions</summary>
        public void RefreshForPlayMode()
        {
            // Don't reset everything - just refresh the current state
            if (m_overlayRoot != null && m_currentContext != null)
            {
                m_needsContentUpdate = true;

                // Ensure the overlay is still attached to scene view
                var sceneView = SceneView.lastActiveSceneView;
                if (sceneView != null && sceneView.rootVisualElement != null)
                {
                    if (!sceneView.rootVisualElement.Contains(m_overlayRoot))
                    {
                        sceneView.rootVisualElement.Add(m_overlayRoot);
                    }
                }
            }
        }


        /// <summary>Reset for new character</summary>
        public void ResetForNewCharacter()
        {
            m_lastSelectedChain = -1;
            m_lastSelectedNode = -1;
            m_needsContentUpdate = true;

            CleanupUIForSelectionChange();
            HideOverlay();

            m_isInitialized = false;
            m_uiComponentsInitialized = false;
            m_overlayRoot = null;
        }

        /// <summary>Complete cleanup for mode transitions</summary>
        public void Cleanup()
        {
            // Unsubscribe from undo events
            Undo.undoRedoPerformed -= OnUndoRedo;
            
            HideOverlay();

            if (m_overlayRoot != null)
            {
                var header = m_overlayRoot.Q<VisualElement>("overlay-header");
                if (header != null)
                {
                    header.UnregisterCallback<MouseDownEvent>(OnDragStart);
                    header.UnregisterCallback<MouseMoveEvent>(OnDragMove);
                    header.UnregisterCallback<MouseUpEvent>(OnDragEnd);
                }

                m_overlayRoot = null;
            }

            m_isInitialized = false;
            m_uiComponentsInitialized = false;
            m_lastSelectedChain = -1;
            m_lastSelectedNode = -1;
            m_needsContentUpdate = true;

            CleanupUIForSelectionChange();
        }

        private void ShowOverlay(RagdollMakerContext ctx, CustomNode node, Transform bone)
        {
            if (m_overlayUXML == null) return;

            if (m_overlayRoot == null)
            {
                CreateOverlayUI();
            }

            if (m_needsContentUpdate)
            {
                UpdateOverlayContent(ctx, node, bone);
                m_needsContentUpdate = false;
            }

            PositionOverlay(ctx);

            var sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null && sceneView.rootVisualElement != null)
            {
                if (!sceneView.rootVisualElement.Contains(m_overlayRoot))
                {
                    sceneView.rootVisualElement.Add(m_overlayRoot);
                }
            }
        }

        private void HideOverlay()
        {
            if (m_overlayRoot?.parent != null)
            {
                m_overlayRoot.RemoveFromHierarchy();
            }

            CleanupUIForSelectionChange();
        }

        private void CreateOverlayUI()
        {
            m_overlayRoot = m_overlayUXML.Instantiate();

            if (m_styleSheet != null)
            {
                m_overlayRoot.styleSheets.Add(m_styleSheet);
            }

            SetupUIReferences();
            SetupEventHandlers();
            SwitchToTab((int)OverlayTab.Collider);
            SetupToolButtons();
            InitializeUIComponents();
        }

        private void InitializeUIComponents()
        {
            if (m_uiComponentsInitialized) return;

            for (int i = 0; i < s_TabUIHandlers.Length; i++)
            {
                if (m_tabPanels[i] != null && s_TabUIHandlers[i] != null)
                {
                    s_TabUIHandlers[i].Initialize(m_tabPanels[i]);
                }
            }

            m_uiComponentsInitialized = true;
        }

        private void SetupUIReferences()
        {
            m_overlayTitle = m_overlayRoot.Q<Label>("overlay-title");
            m_xrayToggle = m_overlayRoot.Q<Button>("xray-toggle");

            m_tabButtons[0] = m_overlayRoot.Q<Button>("collider-tab");
            m_tabButtons[1] = m_overlayRoot.Q<Button>("joint-tab");
            m_tabButtons[2] = m_overlayRoot.Q<Button>("fit-tab");
            m_tabButtons[3] = m_overlayRoot.Q<Button>("mass-tab");

            m_toolContainer = m_overlayRoot.Q<VisualElement>("tool-container");
            m_toolButtons = m_overlayRoot.Q<VisualElement>("tool-buttons");
            m_contentContainer = m_overlayRoot.Q<VisualElement>("content-container");

            m_tabPanels[0] = m_overlayRoot.Q<VisualElement>("collider-panel");
            m_tabPanels[1] = m_overlayRoot.Q<VisualElement>("joint-panel");
            m_tabPanels[2] = m_overlayRoot.Q<VisualElement>("fit-panel");
            m_tabPanels[3] = m_overlayRoot.Q<VisualElement>("mass-panel");
        }

        private void SetupEventHandlers()
        {
            for (int i = 0; i < m_tabButtons.Length; i++)
            {
                if (m_tabButtons[i] != null)
                {
                    int tabIndex = i;
                    m_tabButtons[i].clicked += () => SwitchToTab(tabIndex);
                }
            }

            if (m_xrayToggle != null)
            {
                m_xrayToggle.clicked += ToggleXRay;
            }

            // Subscribe to undo/redo events
            Undo.undoRedoPerformed -= OnUndoRedo;
            Undo.undoRedoPerformed += OnUndoRedo;
        }

        private void OnUndoRedo()
        {
            // Mark that content needs update
            m_needsContentUpdate = true;
            
            // Force repaint of scene views to update the overlay
            EditorApplication.delayCall += () => 
            {
                SceneView.RepaintAll();
            };
        }

        private void SetupToolButtons()
        {
            m_toolButtons.Clear();

            bool showTools = s_TopTabIndex < 2;
            if (!showTools) return;

            string[] tools = s_TopTabIndex == 0 ? s_ColliderTools : s_JointTools;

            for (int i = 0; i < tools.Length; i++)
            {
                var toolBtn = new Button();
                toolBtn.AddToClassList("tool-btn");
                toolBtn.text = tools[i];

                int toolIndex = i;
                toolBtn.clicked += () => SelectTool(toolIndex);

                m_toolButtons.Add(toolBtn);
            }
        }

        private void SwitchToTab(int tabIndex)
        {
            s_TopTabIndex = tabIndex;

            // Update tab visual states
            for (int i = 0; i < m_tabButtons.Length; i++)
            {
                if (m_tabButtons[i] != null)
                {
                    m_tabButtons[i].RemoveFromClassList("tab-btn--active");
                }
            }

            // Update panel visibility
            for (int i = 0; i < m_tabPanels.Length; i++)
            {
                if (m_tabPanels[i] != null)
                {
                    m_tabPanels[i].RemoveFromClassList("panel--active");
                }
            }

            // Update tool container visibility
            bool showTools = tabIndex < 2;
            if (m_toolContainer != null)
            {
                m_toolContainer.style.display = showTools ? DisplayStyle.Flex : DisplayStyle.None;
            }

            // Activate selected tab
            if (tabIndex >= 0 && tabIndex < m_tabButtons.Length && m_tabButtons[tabIndex] != null)
            {
                m_tabButtons[tabIndex].AddToClassList("tab-btn--active");
            }

            if (tabIndex >= 0 && tabIndex < m_tabPanels.Length && m_tabPanels[tabIndex] != null)
            {
                m_tabPanels[tabIndex].AddToClassList("panel--active");
            }

            // Update context active tab
            if (m_currentContext != null)
            {
                m_currentContext.ActiveTab = (OverlayTab)tabIndex;
            }

            SetupToolButtons();
            UpdateToolButtons();
            m_needsContentUpdate = true;
        }

        private void SelectTool(int toolIndex)
        {
            if (m_currentContext == null) return;

            if (s_TopTabIndex == 0)
            {
                m_currentContext.ActiveColliderTool = (ColliderTool)toolIndex;
            }
            else if (s_TopTabIndex == 1)
            {
                m_currentContext.ActiveJointTool = (JointTool)toolIndex;
            }

            UpdateToolButtons();
        }

        private void UpdateToolButtons()
        {
            if (m_currentContext == null) return;

            var buttons = m_toolButtons.Children();
            int activeIndex = s_TopTabIndex == 0
                ? (int)m_currentContext.ActiveColliderTool
                : (int)m_currentContext.ActiveJointTool;

            int index = 0;
            foreach (var button in buttons)
            {
                if (button is Button btn)
                {
                    btn.RemoveFromClassList("tool-btn--active");
                    if (index == activeIndex)
                    {
                        btn.AddToClassList("tool-btn--active");
                    }
                }
                index++;
            }
        }

        private void ToggleXRay()
        {
            if (m_currentContext != null)
            {
                m_currentContext.XRay = !m_currentContext.XRay;

                if (m_currentContext.XRay)
                {
                    m_xrayToggle.AddToClassList("btn--primary");
                    m_xrayToggle.RemoveFromClassList("btn--secondary");
                }
                else
                {
                    m_xrayToggle.RemoveFromClassList("btn--primary");
                    m_xrayToggle.AddToClassList("btn--secondary");
                }

                EditorApplication.delayCall += () => SceneView.RepaintAll();
            }
        }

        private void UpdateOverlayContent(RagdollMakerContext ctx, CustomNode node, Transform bone)
        {
            string title = $"Node [{ctx.SelectedChain}:{ctx.SelectedNode}] — {(bone ? bone.name : "<null>")}";
            if (m_overlayTitle != null)
            {
                m_overlayTitle.text = title;
            }

            if (ctx.XRay)
            {
                m_xrayToggle?.AddToClassList("btn--primary");
                m_xrayToggle?.RemoveFromClassList("btn--secondary");
            }
            else
            {
                m_xrayToggle?.RemoveFromClassList("btn--primary");
                m_xrayToggle?.AddToClassList("btn--secondary");
            }

            UpdateToolButtons();

            // Update only the active tab content
            if (s_TopTabIndex >= 0 && s_TopTabIndex < s_TabUIHandlers.Length &&
                s_TabUIHandlers[s_TopTabIndex] != null &&
                s_TopTabIndex < m_tabPanels.Length && m_tabPanels[s_TopTabIndex] != null)
            {
                s_TabUIHandlers[s_TopTabIndex].UpdateContent(m_tabPanels[s_TopTabIndex], ctx, node);
            }
        }

        private void PositionOverlay(RagdollMakerContext ctx)
        {
            m_overlayRoot.style.position = Position.Absolute;
            m_overlayRoot.style.left = ctx.OverlayRect.x;
            m_overlayRoot.style.top = ctx.OverlayRect.y;
            m_overlayRoot.style.width = ctx.OverlayRect.width;

            SetupDragging();
        }

        private void SetupDragging()
        {
            var header = m_overlayRoot.Q<VisualElement>("overlay-header");
            if (header != null)
            {
                header.RegisterCallback<MouseDownEvent>(OnDragStart);
                header.RegisterCallback<MouseMoveEvent>(OnDragMove);
                header.RegisterCallback<MouseUpEvent>(OnDragEnd);
            }
        }

        private bool m_isDragging = false;
        private Vector2 m_dragOffset;

        private void OnDragStart(MouseDownEvent evt)
        {
            if (evt.button == 0)
            {
                m_isDragging = true;
                m_dragOffset = evt.localMousePosition;
                m_overlayRoot.CaptureMouse();
                evt.StopPropagation();
            }
        }

        private void OnDragMove(MouseMoveEvent evt)
        {
            if (m_isDragging)
            {
                var newPosition = evt.mousePosition - m_dragOffset;
                m_overlayRoot.style.left = newPosition.x;
                m_overlayRoot.style.top = newPosition.y;

                if (m_currentContext != null)
                {
                    m_currentContext.OverlayRect = new Rect(
                        newPosition.x, newPosition.y,
                        m_currentContext.OverlayRect.width,
                        m_currentContext.OverlayRect.height
                    );
                }

                evt.StopPropagation();
            }
        }

        private void OnDragEnd(MouseUpEvent evt)
        {
            if (m_isDragging)
            {
                m_isDragging = false;
                m_overlayRoot.ReleaseMouse();
                evt.StopPropagation();
            }
        }

        #endregion

        #region Constructor & Initialization

        public RagdollOverlayWindow()
        {
            LoadAssets();
        }

        private void LoadAssets()
        {
            m_overlayUXML = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Assets/PhoenixRealm/Ragdoll Creator Pro/Scripts/Editor/RagdollCreatorSystem/OverlayGUI/UIToolkit/UXML_RagdollOverlay.uxml");

            m_styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(
                "Assets/PhoenixRealm/Ragdoll Creator Pro/Scripts/Editor/RagdollCreatorSystem/OverlayGUI/UIToolkit/USS_RagdollOverlay.uss");

            if (m_overlayUXML == null)
            {
                Debug.LogError("[RagdollOverlayWindow] Failed to load UXML file. Check the path: Assets/Scripts/Editor/OverlayGUI/UIToolkit/UXML_RagdollOverlay.uxml");
            }

            if (m_styleSheet == null)
            {
                Debug.LogWarning("[RagdollOverlayWindow] Failed to load USS file. Check the path: Assets/Scripts/Editor/OverlayGUI/UIToolkit/USS_RagdollOverlay.uss");
            }
        }

        #endregion
    }
}
#endif
