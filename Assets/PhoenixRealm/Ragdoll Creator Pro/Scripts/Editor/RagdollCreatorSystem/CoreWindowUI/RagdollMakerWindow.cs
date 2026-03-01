#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;
using PhoenixRealm.RagdollCreatorPro;

namespace PhoenixRealm.RagdollCreatorPro.Editor
{
    public partial class RagdollMakerWindow : EditorWindow
    {
        #region Vars + Properties

        [SerializeField] private RagdollMakerContext m_ctx;

        // UI Components
        private RagdollWindowUI m_windowUI;
        private RagdollSceneManager m_sceneManager;
        private RagdollCharacterManager m_characterManager;
        private RagdollChainManager m_chainManager;
        private RagdollValidationManager m_validationManager;
        private bool m_isFullyInitialized = false;
        private static RagdollMakerWindow s_currentInstance;

        #endregion

        #region Unity Functions

        [MenuItem("Tools/PhoenixRealm/RagdollCreatorPro Maker")]
        public static void ShowWindow()
        {
            var w = GetWindow<RagdollMakerWindow>("RagdollCreatorPro Maker");
            s_currentInstance = w;
            w.Show();
        }

        public void CreateGUI()
        {
            InitializeComponents();
            m_windowUI.CreateGUI(this, rootVisualElement);
            m_windowUI.SetCharacterManager(m_characterManager);
            m_isFullyInitialized = true;
        }

        private void OnEnable()
        {
            s_currentInstance = this;
            InitializeContext();
            InitializeComponents();

            EditorApplication.delayCall += DelayedInitialization;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                m_sceneManager?.DisableSceneDrawing();
            }
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            if (s_currentInstance == this)
            {
                s_currentInstance = null;
            }
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.ExitingEditMode:
                    m_sceneManager?.DisableSceneDrawing();
                    break;
                case PlayModeStateChange.EnteredEditMode:
                    EditorApplication.delayCall += CloseAndReopenWindow;
                    break;
            }
        }

        #endregion

        #region Close and Reopen Implementation

        private static void CloseAndReopenWindow()
        {
            Debug.Log("[RagdollCreatorPro Maker] Closing and reopening window for fresh start...");

            // Store current window position and size for reopening
            Rect windowRect = new Rect(100, 100, 800, 600); // Default values
            bool wasWindowOpen = false;

            if (s_currentInstance != null)
            {
                // Get current window position and size
                windowRect = s_currentInstance.position;
                wasWindowOpen = true;

                // Close the current window completely
                s_currentInstance.Close();
                s_currentInstance = null;
            }

            // Only reopen if a window was actually open
            if (wasWindowOpen)
            {
                // Wait a frame then reopen with fresh state
                EditorApplication.delayCall += () => {
                    var newWindow = GetWindow<RagdollMakerWindow>("RagdollCreatorPro Maker");

                    // Restore window position and size
                    newWindow.position = windowRect;

                    // Focus the new window
                    newWindow.Focus();

                    Debug.Log("[RagdollCreatorPro Maker] Window reopened with completely fresh state.");
                };
            }
        }

        private void DelayedInitialization()
        {
            if (m_ctx != null)
            {
                if (m_ctx.TargetCharacter == null)
                {
                    m_characterManager?.AutoAssignSelectedCharacter();
                }
                m_sceneManager?.EnableSceneDrawing();
                UpdateUI();
            }
        }

        #endregion

        #region Initialization

        private void InitializeContext()
        {
            if (m_ctx == null)
            {
                m_ctx = CreateInstance<RagdollMakerContext>();
                m_ctx.name = "RagdollMakerContext (EditorOnly)";
            }
        }

        private void InitializeComponents()
        {
            if (m_windowUI == null)
            {
                m_windowUI = new RagdollWindowUI();
                m_sceneManager = new RagdollSceneManager(m_ctx);
                m_characterManager = new RagdollCharacterManager(m_ctx);
                m_chainManager = new RagdollChainManager(m_ctx);
                m_validationManager = new RagdollValidationManager(m_ctx);

                SetupComponentInteractions();
            }
        }

        private void SetupComponentInteractions()
        {
            m_characterManager.OnCharacterChanged += OnCharacterChanged;
            m_chainManager.OnChainsChanged += OnChainsChanged;
            m_sceneManager.OnSelectionChanged += OnSceneSelectionChanged;
        }

        #endregion

        #region Presets
        public void CreateFromPreset(RagdollPresetBase preset)
        {
            if (preset == null)
            {
                Debug.LogError("[RagdollCreatorPro Maker] Cannot create from null preset");
                return;
            }

            if (m_ctx.TargetCharacter == null)
            {
                Debug.LogError("[RagdollCreatorPro Maker] No character assigned. Please assign a character first.");
                return;
            }

            Undo.RecordObject(m_ctx, "Create Chains From Preset");

            m_ctx.Chains.Clear();
            var newChains = preset.CreateChainsFromPreset();
            m_ctx.Chains.AddRange(newChains);

            m_chainManager.OnChainsChanged?.Invoke();

            Debug.Log($"[RagdollCreatorPro Maker] Created {newChains.Count} chains from preset '{preset.PresetName}'");
        }

        public void SaveCurrentAsPreset()
        {
            if (m_ctx.Chains == null || m_ctx.Chains.Count == 0)
            {
                EditorUtility.DisplayDialog("Cannot Save Preset",
                    "No chains configured. Please create chains before saving a preset.", "OK");
                return;
            }

            RagdollPresetCreator.CreatePresetFromCurrentChains(m_ctx.Chains);
        }

        #endregion

        #region Event Handlers

        private void OnCharacterChanged()
        {
            m_sceneManager.OnCharacterChanged();
            UpdateUI();
        }

        private void OnChainsChanged()
        {
            m_validationManager.RefreshValidation();
            UpdateUI();
        }

        private void OnSceneSelectionChanged()
        {
            m_characterManager.HandleSceneSelectionChanged();
            Repaint();
            SceneView.RepaintAll();
        }

        public void UpdateUI()
        {
            m_windowUI?.UpdateUI(m_ctx, m_characterManager, m_chainManager, m_validationManager);
        }

        #endregion

        #region Public API for UI Components

        public void BakeRagdoll() => m_characterManager.BakeRagdoll();
        public void SyncFromCharacter() => m_characterManager.SyncFromCharacter();
        public void SyncToCharacter() => m_characterManager.SyncToCharacter();
        public void CreateRagdollMap() => m_characterManager.CreateRagdollMap();
        public void AddNewChain() => m_chainManager.AddNewChain();
        public void DuplicateChain(int chainIndex) => m_chainManager.DuplicateChain(chainIndex);

        #endregion
    }
}
#endif
