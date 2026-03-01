#if UNITY_EDITOR
using UnityEngine.UIElements;

namespace PhoenixRealm.RagdollCreatorPro.Editor
{
    /// <summary>Shared interface for all overlay tab UI implementations</summary>
    internal interface IOverlayTabUI
    {
        /// <summary>Initialize the UI elements for this tab</summary>
        void Initialize(VisualElement tabPanel);

        /// <summary>Update the content when node or context changes</summary>
        void UpdateContent(VisualElement tabPanel, RagdollMakerContext ctx, CustomNode node);

        /// <summary>Clean up callbacks and state when switching tabs or closing</summary>
        void Cleanup();

        /// <summary>Returns true if this tab UI is properly initialized</summary>
        bool IsInitialized { get; }
    }
}
#endif
