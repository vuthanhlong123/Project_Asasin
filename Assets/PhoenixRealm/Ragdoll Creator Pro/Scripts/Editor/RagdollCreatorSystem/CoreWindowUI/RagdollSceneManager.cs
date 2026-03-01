#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;
using PhoenixRealm.RagdollCreatorPro;

namespace PhoenixRealm.RagdollCreatorPro.Editor
{
    public class RagdollSceneManager
    {
        #region Vars + Properties

        private RagdollMakerContext m_ctx;
        private RagdollSceneDrawer m_sceneDrawer;
        private RagdollOverlayWindow m_overlay;

        public System.Action OnSelectionChanged;

        #endregion

        #region Constructor

        public RagdollSceneManager(RagdollMakerContext ctx)
        {
            m_ctx = ctx;
        }

        #endregion

        #region Public API

        public void EnableSceneDrawing()
        {
            if (m_ctx == null) return;

            InitializeSceneHelpers();
            RegisterCallbacks();
            SceneView.RepaintAll();
        }

        public void DisableSceneDrawing()
        {
            UnregisterCallbacks();
            HideOverlayFromAllSceneViews();
            ResetOverlayState();
            SceneView.RepaintAll();
        }

        public void OnCharacterChanged()
        {
            if (m_overlay != null)
            {
                m_overlay.ResetForNewCharacter();
            }
            SceneView.RepaintAll();
        }

        #endregion

        #region Scene Drawing

        private void InitializeSceneHelpers()
        {
            if (m_sceneDrawer == null)
                m_sceneDrawer = new RagdollSceneDrawer();

            if (m_overlay == null)
                m_overlay = new RagdollOverlayWindow();
        }

        private void ResetOverlayState()
        {
            if (m_overlay != null)
            {
                m_overlay.Cleanup();
                m_overlay = null;
            }
        }

        private void RegisterCallbacks()
        {
            SceneView.duringSceneGui -= DuringSceneGUI;
            SceneView.duringSceneGui += DuringSceneGUI;

            RagdollSceneSelection.SelectionChanged -= HandleSelectionChanged;
            RagdollSceneSelection.SelectionChanged += HandleSelectionChanged;
        }

        private void UnregisterCallbacks()
        {
            SceneView.duringSceneGui -= DuringSceneGUI;
            RagdollSceneSelection.SelectionChanged -= HandleSelectionChanged;
        }

        private void DuringSceneGUI(UnityEditor.SceneView sv)
        {
            if (m_ctx == null || m_sceneDrawer == null) return;

            m_sceneDrawer.DrawScene(m_ctx);
            m_overlay?.Draw(m_ctx);
        }

        private void HandleSelectionChanged()
        {
            OnSelectionChanged?.Invoke();
        }

        private void HideOverlayFromAllSceneViews()
        {
            var sceneViews = Resources.FindObjectsOfTypeAll<UnityEditor.SceneView>();

            foreach (var sceneView in sceneViews)
            {
                if (sceneView?.rootVisualElement != null)
                {
                    var children = sceneView.rootVisualElement.Children().ToList();
                    foreach (var child in children)
                    {
                        if (IsRagdollOverlayElement(child))
                        {
                            child.RemoveFromHierarchy();
                        }
                    }
                }
            }
        }

        private bool IsRagdollOverlayElement(UnityEngine.UIElements.VisualElement element)
        {
            if (element == null) return false;

            var elementClasses = element.GetClasses();
            return elementClasses.Contains("ragdoll-overlay") ||
                   elementClasses.Contains("overlay-root") ||
                   element.name?.Contains("ragdoll") == true ||
                   element.name?.Contains("overlay") == true;
        }

        #endregion
    }
}
#endif
