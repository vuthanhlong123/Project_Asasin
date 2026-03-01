#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;

namespace PhoenixRealm.RagdollCreatorPro.Editor
{
    public class HitboxSetupWindow : EditorWindow
    {
        #region Vars + Properties

        private RagdollMap m_sourceRagdollMap;
        private GameObject m_targetCharacter;
        private GameObject m_hitboxPrefab;
        private bool m_isTrigger = true;
        private bool m_showScenePreview = false;

        private VisualElement m_root;
        private ObjectField m_ragdollMapField;
        private ObjectField m_targetCharacterField;
        private ObjectField m_hitboxPrefabField;
        private Toggle m_isTriggerToggle;
        private Toggle m_showPreviewToggle;
        private Button m_applyButton;
        private ScrollView m_previewScrollView;
        private VisualElement m_previewList;
        private Label m_statsLabel;
        private HelpBox m_helpBox;

        private List<HitboxPreview> m_previewData = new List<HitboxPreview>();
        private HitboxSceneDrawer m_sceneDrawer;

        #endregion

        #region Unity Functions

        [MenuItem("Tools/PhoenixRealm/Hitbox Setup")]
        public static void ShowWindow()
        {
            var window = GetWindow<HitboxSetupWindow>("Hitbox Setup");
            window.minSize = new Vector2(450, 500);
            window.Show();
        }

        public void CreateGUI()
        {
            if (!LoadUXMLTemplate())
            {
                CreateFallbackUI();
                return;
            }

            SetupUIReferences();
            SetupEventHandlers();
            UpdatePreview();
            UpdateUI();
        }

        private void OnEnable()
        {
            m_sceneDrawer = new HitboxSceneDrawer();
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            m_sceneDrawer = null;
        }

        #endregion

        #region UI Initialization

        private bool LoadUXMLTemplate()
        {
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Assets/PhoenixRealm/Ragdoll Creator Pro/Scripts/Editor/HitboxSystem/UIToolkit/UXML_HitboxSetupWindow.uxml");

            if (visualTree == null)
            {
                Debug.LogError("[Hitbox Setup] UXML file not found at expected path!");
                return false;
            }

            m_root = visualTree.Instantiate();
            rootVisualElement.Add(m_root);
            return true;
        }

        private void CreateFallbackUI()
        {
            var label = new Label("Hitbox Setup - UXML file missing!");
            label.style.color = Color.red;
            label.style.fontSize = 16;
            label.style.unityTextAlign = TextAnchor.MiddleCenter;
            rootVisualElement.Add(label);
        }

        private void SetupUIReferences()
        {
            m_ragdollMapField = m_root.Q<ObjectField>("ragdoll-map-field");
            m_targetCharacterField = m_root.Q<ObjectField>("target-character-field");
            m_hitboxPrefabField = m_root.Q<ObjectField>("hitbox-prefab-field");
            m_isTriggerToggle = m_root.Q<Toggle>("is-trigger-toggle");
            m_showPreviewToggle = m_root.Q<Toggle>("show-preview-toggle");
            m_applyButton = m_root.Q<Button>("apply-button");
            m_previewScrollView = m_root.Q<ScrollView>("preview-scroll");
            m_previewList = m_root.Q<VisualElement>("preview-list");
            m_statsLabel = m_root.Q<Label>("stats-label");
            m_helpBox = m_root.Q<HelpBox>("help-box");
        }

        private void SetupEventHandlers()
        {
            if (m_ragdollMapField != null)
            {
                m_ragdollMapField.objectType = typeof(RagdollMap);
                m_ragdollMapField.RegisterValueChangedCallback(evt =>
                {
                    m_sourceRagdollMap = evt.newValue as RagdollMap;
                    Debug.Log($"[Hitbox Setup] RagdollMap changed: {(m_sourceRagdollMap != null ? m_sourceRagdollMap.name : "null")}");
                    OnSettingsChanged();
                });
            }

            if (m_targetCharacterField != null)
            {
                m_targetCharacterField.objectType = typeof(GameObject);
                m_targetCharacterField.RegisterValueChangedCallback(evt =>
                {
                    m_targetCharacter = evt.newValue as GameObject;
                    Debug.Log($"[Hitbox Setup] Target character changed: {(m_targetCharacter != null ? m_targetCharacter.name : "null")}");
                    OnSettingsChanged();
                });
            }

            if (m_hitboxPrefabField != null)
            {
                m_hitboxPrefabField.objectType = typeof(GameObject);
                m_hitboxPrefabField.RegisterValueChangedCallback(evt =>
                {
                    m_hitboxPrefab = evt.newValue as GameObject;
                });
            }

            if (m_isTriggerToggle != null)
            {
                m_isTriggerToggle.RegisterValueChangedCallback(evt =>
                {
                    m_isTrigger = evt.newValue;
                });
            }

            if (m_showPreviewToggle != null)
            {
                m_showPreviewToggle.RegisterValueChangedCallback(evt =>
                {
                    m_showScenePreview = evt.newValue;
                    SceneView.RepaintAll();
                });
            }

            if (m_applyButton != null)
            {
                m_applyButton.clicked += ApplyHitboxes;
                Debug.Log("[Hitbox Setup] Apply button event handler registered");
            }
        }

        #endregion

        #region Event Handlers

        private void OnSettingsChanged()
        {
            UpdatePreview();
            UpdateUI();
        }

        #endregion

        #region Preview System

        private void UpdatePreview()
        {
            m_previewData.Clear();

            if (!CanGeneratePreview())
            {
                UpdatePreviewUI();
                return;
            }

            var chains = m_sourceRagdollMap.Chains;
            if (chains == null || chains.Count == 0)
            {
                Debug.LogWarning("[Hitbox Setup] RagdollMap has no chains configured.");
                UpdatePreviewUI();
                return;
            }

            Transform characterRoot = m_targetCharacter.transform;

            foreach (var chain in chains)
            {
                if (chain?.Nodes == null)
                    continue;

                foreach (var nodeData in chain.Nodes)
                {
                    if (nodeData == null)
                        continue;

                    Transform targetBone = FindBoneByName(characterRoot, nodeData.NodeName);

                    var preview = new HitboxPreview
                    {
                        BoneName = nodeData.NodeName,
                        TargetBone = targetBone,
                        ColliderType = nodeData.ColliderType,
                        LocalOffset = nodeData.LocalOffset,
                        ColliderRadius = nodeData.ColliderRadius,
                        ColliderHeight = nodeData.ColliderHeight,
                        ColliderSize = nodeData.ColliderSize,
                        CapsuleDirection = nodeData.CapsuleDirection
                    };

                    m_previewData.Add(preview);
                }
            }

            Debug.Log($"[Hitbox Setup] Generated {m_previewData.Count} preview entries");
            UpdatePreviewUI();
        }

        private Transform FindBoneByName(Transform root, string boneName)
        {
            if (string.IsNullOrEmpty(boneName))
                return null;

            if (root.name == boneName)
                return root;

            foreach (Transform child in root)
            {
                Transform result = FindBoneByName(child, boneName);
                if (result != null)
                    return result;
            }

            return null;
        }

        #endregion

        #region UI Update

        private void UpdateUI()
        {
            bool canApply = CanApplyHitboxes();

            if (m_applyButton != null)
            {
                m_applyButton.SetEnabled(canApply);
                Debug.Log($"[Hitbox Setup] Apply button enabled: {canApply}, Preview data count: {m_previewData.Count}");
            }

            if (m_previewScrollView != null)
            {
                bool shouldShow = CanGeneratePreview();
                m_previewScrollView.style.display = shouldShow ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private void UpdatePreviewUI()
        {
            if (m_previewList == null)
                return;

            m_previewList.Clear();

            if (m_previewData.Count == 0)
            {
                var emptyLabel = new Label("No hitboxes to create. Check bone name matching.");
                emptyLabel.AddToClassList("preview-empty");
                m_previewList.Add(emptyLabel);
                UpdateStats(0, 0);
                return;
            }

            int successCount = 0;
            int failCount = 0;

            foreach (var preview in m_previewData)
            {
                var row = CreatePreviewRow(preview);
                m_previewList.Add(row);

                if (preview.TargetBone != null)
                    successCount++;
                else
                    failCount++;
            }

            UpdateStats(successCount, failCount);
        }

        private VisualElement CreatePreviewRow(HitboxPreview preview)
        {
            var row = new VisualElement();
            row.AddToClassList("preview-row");

            bool isMatched = preview.TargetBone != null;

            var statusIcon = new Label(isMatched ? "✓" : "✗");
            statusIcon.AddToClassList("preview-icon");
            statusIcon.AddToClassList(isMatched ? "preview-icon--success" : "preview-icon--error");

            var boneName = new Label(preview.BoneName);
            boneName.AddToClassList("preview-bone-name");

            var colliderType = new Label(preview.ColliderType.ToString());
            colliderType.AddToClassList("preview-collider-type");

            row.Add(statusIcon);
            row.Add(boneName);
            row.Add(colliderType);

            if (isMatched)
            {
                var focusBtn = new Button(() => FocusBone(preview.TargetBone))
                {
                    text = "Focus"
                };
                focusBtn.AddToClassList("btn");
                focusBtn.AddToClassList("btn--secondary");
                focusBtn.AddToClassList("preview-focus-btn");
                row.Add(focusBtn);
            }

            return row;
        }

        private void UpdateStats(int successCount, int failCount)
        {
            if (m_statsLabel == null)
                return;

            int total = successCount + failCount;
            m_statsLabel.text = $"Total: {total}  |  ✓ Matched: {successCount}  |  ✗ Not Found: {failCount}";
        }

        private void FocusBone(Transform bone)
        {
            Selection.activeGameObject = bone.gameObject;
            SceneView.FrameLastActiveSceneView();
        }

        #endregion

        #region Validation

        private bool CanGeneratePreview()
        {
            return m_sourceRagdollMap != null && m_targetCharacter != null;
        }

        private bool CanApplyHitboxes()
        {
            if (!CanGeneratePreview())
            {
                Debug.Log("[Hitbox Setup] Cannot apply: Missing RagdollMap or Target Character");
                return false;
            }

            if (m_previewData.Count == 0)
            {
                Debug.Log("[Hitbox Setup] Cannot apply: Preview data is empty");
                return false;
            }

            int matchedCount = 0;
            foreach (var preview in m_previewData)
            {
                if (preview.TargetBone != null)
                    matchedCount++;
            }

            if (matchedCount == 0)
            {
                Debug.Log("[Hitbox Setup] Cannot apply: No bones matched");
                return false;
            }

            return true;
        }

        #endregion

        #region Apply System

        private void ApplyHitboxes()
        {
            Debug.Log("[Hitbox Setup] ApplyHitboxes called!");

            if (!CanApplyHitboxes())
            {
                Debug.LogWarning("[Hitbox Setup] Cannot apply hitboxes - validation failed");
                EditorUtility.DisplayDialog(
                    "Cannot Apply",
                    "Please ensure:\n" +
                    "• RagdollMap is assigned\n" +
                    "• Target Character is assigned\n" +
                    "• At least one bone name matches",
                    "OK");
                return;
            }

            Undo.SetCurrentGroupName("Apply Hitboxes");
            int undoGroup = Undo.GetCurrentGroup();

            int successCount = 0;
            int failCount = 0;

            foreach (var preview in m_previewData)
            {
                if (preview.TargetBone == null)
                {
                    failCount++;
                    continue;
                }

                GameObject hitboxGO = CreateHitboxGameObject(preview);
                if (hitboxGO == null)
                {
                    Debug.LogError($"[Hitbox Setup] Failed to create hitbox GameObject for bone: {preview.BoneName}");
                    failCount++;
                    continue;
                }

                Collider collider = AddColliderComponent(hitboxGO, preview);
                if (collider == null)
                {
                    Debug.LogError($"[Hitbox Setup] Failed to add collider for bone: {preview.BoneName}");
                    Object.DestroyImmediate(hitboxGO);
                    failCount++;
                    continue;
                }

                collider.isTrigger = m_isTrigger;

                Undo.RegisterCreatedObjectUndo(hitboxGO, "Create Hitbox");
                successCount++;
                Debug.Log($"[Hitbox Setup] Created hitbox for bone: {preview.BoneName}");
            }

            Undo.CollapseUndoOperations(undoGroup);

            Debug.Log($"[Hitbox Setup] Complete - Success: {successCount}, Failed: {failCount}");

            EditorUtility.DisplayDialog(
                "Hitbox Setup Complete",
                $"Successfully created {successCount} hitboxes.\n" +
                $"{failCount} failed (bone not found or error).",
                "OK");

            m_showScenePreview = false;
            if (m_showPreviewToggle != null)
            {
                m_showPreviewToggle.SetValueWithoutNotify(false);
            }
            SceneView.RepaintAll();
        }

        private GameObject CreateHitboxGameObject(HitboxPreview preview)
        {
            GameObject hitboxGO;

            if (m_hitboxPrefab != null)
            {
                hitboxGO = PrefabUtility.InstantiatePrefab(m_hitboxPrefab) as GameObject;
            }
            else
            {
                hitboxGO = new GameObject();
            }

            if (hitboxGO == null)
                return null;

            hitboxGO.name = $"Hitbox_{preview.BoneName}";
            hitboxGO.transform.SetParent(preview.TargetBone, false);
            hitboxGO.transform.localPosition = preview.LocalOffset;
            hitboxGO.transform.localRotation = Quaternion.identity;
            hitboxGO.transform.localScale = Vector3.one;

            return hitboxGO;
        }

        private Collider AddColliderComponent(GameObject go, HitboxPreview preview)
        {
            Collider collider = null;

            switch (preview.ColliderType)
            {
                case ColliderType.Sphere:
                    var sphereCol = go.AddComponent<SphereCollider>();
                    sphereCol.radius = preview.ColliderRadius;
                    collider = sphereCol;
                    break;

                case ColliderType.Capsule:
                    var capsuleCol = go.AddComponent<CapsuleCollider>();
                    capsuleCol.radius = preview.ColliderRadius;
                    capsuleCol.height = preview.ColliderHeight + preview.ColliderRadius * 2f;
                    capsuleCol.direction = (int)preview.CapsuleDirection;
                    collider = capsuleCol;
                    break;

                case ColliderType.Box:
                    var boxCol = go.AddComponent<BoxCollider>();
                    boxCol.size = preview.ColliderSize;
                    collider = boxCol;
                    break;
            }

            return collider;
        }

        #endregion

        #region Scene Drawing

        private void OnSceneGUI(SceneView sceneView)
        {
            if (!m_showScenePreview || m_previewData.Count == 0 || m_sceneDrawer == null)
                return;

            m_sceneDrawer.DrawHitboxPreviews(m_previewData);
        }

        #endregion

        #region Helper Classes

        internal class HitboxPreview
        {
            public string BoneName;
            public Transform TargetBone;
            public ColliderType ColliderType;
            public Vector3 LocalOffset;
            public float ColliderRadius;
            public float ColliderHeight;
            public Vector3 ColliderSize;
            public CapsuleDirection CapsuleDirection;
        }

        #endregion
    }
}
#endif
