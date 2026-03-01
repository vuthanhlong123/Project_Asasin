#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;

namespace PhoenixRealm.RagdollCreatorPro.Editor
{
    public class RagdollWindowUI
    {
        #region Vars + Properties

        private VisualElement m_root;
        private Image m_bannerImageElement;
        private ObjectField m_characterField;
        private Button m_createRagdollBtn;
        private Label m_characterStatusText;
        private VisualElement m_syncButtons;
        private Button m_syncFromBtn;
        private Button m_syncToBtn;
        private Button m_bakeBtn;
        private Button m_addChainBtn;
        private VisualElement m_chainsContainer;
        private VisualElement m_validationContainer;
        private VisualElement m_presetButtons;
        private VisualElement m_chainsSection;
        private VisualElement m_validationSection;

        private RagdollMakerWindow m_window;
        private RagdollCharacterManager m_characterManager;

        #endregion

        #region Public API

        public void CreateGUI(RagdollMakerWindow window, VisualElement rootElement)
        {
            m_window = window;

            if (!LoadUXMLTemplate(rootElement))
            {
                CreateFallbackUI(rootElement);
                return;
            }

            SetupUIReferences();
            SetupEventHandlers();
            LoadBannerImage();
        }

        public void SetCharacterManager(RagdollCharacterManager characterManager)
        {
            m_characterManager = characterManager;
        }

        public void UpdateUI(RagdollMakerContext ctx, RagdollCharacterManager characterManager,
                           RagdollChainManager chainManager, RagdollValidationManager validationManager)
        {
            UpdateCharacterSection(ctx, characterManager);
            UpdateChainsSection(ctx, chainManager);
            UpdateValidationSection(ctx, validationManager);
        }

        #endregion

        #region Initialization

        private bool LoadUXMLTemplate(VisualElement rootElement)
        {
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Assets/PhoenixRealm/Ragdoll Creator Pro/Scripts/Editor/RagdollCreatorSystem/CoreWindowUI/UIToolkit/UXML_RagdollMakerWindow.uxml");

            if (visualTree == null) return false;

            m_root = visualTree.Instantiate();
            rootElement.Add(m_root);
            return true;
        }

        private void CreateFallbackUI(VisualElement rootElement)
        {
            var label = new Label("RagdollCreatorPro Maker - UXML file missing!");
            label.style.color = Color.red;
            label.style.fontSize = 16;
            label.style.unityTextAlign = TextAnchor.MiddleCenter;
            rootElement.Add(label);
        }

        private void SetupUIReferences()
        {
            m_bannerImageElement = m_root.Q<Image>("banner-image");
            m_characterField = m_root.Q<ObjectField>("character-field");
            m_createRagdollBtn = m_root.Q<Button>("create-ragdoll-btn");
            m_characterStatusText = m_root.Q<Label>("character-status-text");
            m_syncButtons = m_root.Q<VisualElement>("sync-buttons");
            m_syncFromBtn = m_root.Q<Button>("sync-from-btn");
            m_syncToBtn = m_root.Q<Button>("sync-to-btn");
            m_bakeBtn = m_root.Q<Button>("bake-btn");
            m_addChainBtn = m_root.Q<Button>("add-chain-btn");
            m_chainsContainer = m_root.Q<VisualElement>("chains-container");
            m_validationContainer = m_root.Q<VisualElement>("validation-container");
            m_presetButtons = m_root.Q<VisualElement>("preset-buttons");
            m_chainsSection = m_root.Q<VisualElement>("chains-section");
            m_validationSection = m_root.Q<VisualElement>("validation-section");
        }

        private void SetupEventHandlers()
        {
            if (m_characterField != null)
            {
                m_characterField.RegisterValueChangedCallback(evt =>
                {
                    var newCharacter = evt.newValue as GameObject;
                    HandleCharacterFieldChanged(newCharacter);
                });
            }

            if (m_createRagdollBtn != null) m_createRagdollBtn.clicked += m_window.CreateRagdollMap;
            if (m_syncFromBtn != null) m_syncFromBtn.clicked += m_window.SyncFromCharacter;
            if (m_syncToBtn != null) m_syncToBtn.clicked += m_window.SyncToCharacter;
            if (m_bakeBtn != null) m_bakeBtn.clicked += m_window.BakeRagdoll;
            if (m_addChainBtn != null) m_addChainBtn.clicked += m_window.AddNewChain;
        }

        private void HandleCharacterFieldChanged(GameObject newCharacter)
        {
            if (m_characterManager != null)
            {
                m_characterManager.SetTargetCharacter(newCharacter);
            }
        }

        private void LoadBannerImage()
        {
            if (m_bannerImageElement == null) return;

            var bannerImage = TryLoadBannerFromPaths();
            if (bannerImage != null)
            {
                m_bannerImageElement.image = bannerImage;
            }

            // ensure it always fills the container
            var bannerContainer = m_root.Q<VisualElement>("banner-container");
            if (bannerContainer != null && m_bannerImageElement != null)
            {
                m_bannerImageElement.scaleMode = ScaleMode.ScaleAndCrop;
                m_bannerImageElement.StretchToParentSize(); // pins to all 4 edges
            }

            // if a parent imposes padding/margins that create side gaps, zero them:
            if (bannerContainer != null)
            {
                bannerContainer.style.marginLeft = 0;
                bannerContainer.style.marginRight = 0;
                bannerContainer.style.paddingLeft = 0;
                bannerContainer.style.paddingRight = 0;
            }

        }

        private Texture2D TryLoadBannerFromPaths()
        {
            string[] possiblePaths = {
                "Assets/PhoenixRealm/Ragdoll Creator Pro/Sprites/Editor/RagdollMakerBanner.png",
            };

            foreach (var path in possiblePaths)
            {
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (texture != null) return texture;
            }

            return null;
        }

        #endregion

        #region UI Updates

        private void UpdateCharacterSection(RagdollMakerContext ctx, RagdollCharacterManager characterManager)
        {
            if (m_characterField == null || m_characterStatusText == null) return;

            m_characterField.SetValueWithoutNotify(ctx.TargetCharacter);

            if (ctx.TargetCharacter == null)
            {
                HandleNoCharacterAssigned();
            }
            else
            {
                HandleCharacterAssigned(ctx);
            }
        }

        private void HandleNoCharacterAssigned()
        {
            m_characterStatusText.text = "No character assigned - Please assign a character to continue";

            // Hide all functionality when no character is assigned
            SetElementVisibility(m_createRagdollBtn, false);
            SetElementVisibility(m_syncButtons, false);
            SetElementVisibility(m_syncFromBtn, false);
            SetElementVisibility(m_syncToBtn, false);
            SetElementVisibility(m_bakeBtn, false);
            SetElementVisibility(m_chainsSection, false);
            SetElementVisibility(m_validationSection, false);
        }

        private void HandleCharacterAssigned(RagdollMakerContext ctx)
        {
            var map = ctx.TargetCharacter.GetComponent<RagdollMap>();

            if (map == null)
            {
                m_characterStatusText.text = $"Character '{ctx.TargetCharacter.name}' needs a RagdollCreatorPro Map component";

                // Show create button, hide load/save
                SetElementVisibility(m_createRagdollBtn, true);
                SetElementVisibility(m_syncButtons, false);
                SetElementVisibility(m_syncFromBtn, false);
                SetElementVisibility(m_syncToBtn, false);
                SetElementVisibility(m_bakeBtn, false);
                SetElementVisibility(m_chainsSection, false);
                SetElementVisibility(m_validationSection, false);
            }
            else
            {
                m_characterStatusText.text = $"Bound to '{ctx.TargetCharacter.name}' - Ready for chain editing";

                // Hide create button, show load/save/bake and chains
                SetElementVisibility(m_createRagdollBtn, false);
                SetElementVisibility(m_syncButtons, true);
                SetElementVisibility(m_syncFromBtn, true);
                SetElementVisibility(m_syncToBtn, true);
                SetElementVisibility(m_bakeBtn, true);
                SetElementVisibility(m_chainsSection, true);
                SetElementVisibility(m_validationSection, true);
            }
        }

        private void UpdateChainsSection(RagdollMakerContext ctx, RagdollChainManager chainManager)
        {
            if (m_chainsContainer == null) return;

            m_chainsContainer.Clear();

            if (m_presetButtons != null)
            {
                m_presetButtons.Clear();
                var presetButtonsElement = chainManager.CreatePresetButtons(m_window);
                m_presetButtons.Add(presetButtonsElement);
            }

            chainManager.BuildChainsUI(m_chainsContainer);
        }


        private void UpdateValidationSection(RagdollMakerContext ctx, RagdollValidationManager validationManager)
        {
            if (m_validationContainer == null) return;

            m_validationContainer.Clear();
            validationManager.BuildValidationUI(m_validationContainer, m_validationSection);
        }

        private void SetElementVisibility(VisualElement element, bool visible)
        {
            if (element != null)
            {
                element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        #endregion
    }
}
#endif
