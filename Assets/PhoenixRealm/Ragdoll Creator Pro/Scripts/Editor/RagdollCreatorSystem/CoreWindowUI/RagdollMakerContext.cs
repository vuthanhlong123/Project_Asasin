#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PhoenixRealm.RagdollCreatorPro.Editor
{
    /// <summary>Unified overlay tab enumeration with all available tabs</summary>
    public enum OverlayTab
    {
        Collider = 0,
        Joint = 1,
        Fit = 2,
        Mass = 3
    }

    public enum JointTool
    {
        Anchor = 0,
        Axis = 1,
        Twist = 2,
        Swing = 3
    }

    public enum JointAxisBasis
    {
        BoneRight,
        BoneUp,
        BoneForward,
    }
    public enum ColliderTool { Move, Rotate, Scale }

    /// <summary>Shared editor state for the RagdollCreatorPro Maker with fresh restart capability.</summary>
    public sealed class RagdollMakerContext : ScriptableObject
    {
        #region Vars + Properties
        // Data
        public List<CustomChain> Chains = new List<CustomChain>();

        // Selection (indices)
        public int SelectedChain = -1;
        public int SelectedNode = -1;

        // Character binding
        public GameObject TargetCharacter { get; set; }
        public RagdollMap TargetMap { get; set; }

        // Overlay
        public Rect OverlayRect = new Rect(16, 16, 380, 360);
        public OverlayTab ActiveTab = OverlayTab.Collider;

        // Collider tools
        public ColliderTool ActiveColliderTool = ColliderTool.Move;

        // Joint tools
        public JointTool ActiveJointTool = JointTool.Anchor;

        // Visuals
        public bool XRay = true;

        // Validation
        public ValidationResult Validation = new ValidationResult();

        #endregion

        #region Play Mode State Management

        /// <summary>Called when entering play mode to preserve minimal state</summary>
        public void PreservePlayModeState()
        {
            // We don't preserve much since we want a fresh start on exit
            EditorUtility.SetDirty(this);
        }

        /// <summary>Called when exiting play mode - validates references</summary>
        public void RestoreEditModeState()
        {
            // Basic validation only
            ValidateReferences();
        }

        /// <summary>Validates that references are still valid</summary>
        public bool ValidateReferences()
        {
            bool isValid = true;

            // Check if character reference is still valid
            if (TargetCharacter != null && TargetCharacter == null)
            {
                TargetCharacter = null;
                isValid = false;
            }

            // Check if map reference is still valid
            if (TargetMap != null && TargetMap == null)
            {
                TargetMap = null;
                isValid = false;
            }

            return isValid;
        }

        #endregion

        #region Notification System

        /// <summary>Event fired when a node's properties are modified externally (e.g., via scene handles)</summary>
        public System.Action<CustomNode> OnNodeModified;

        /// <summary>Event fired when a chain's properties are modified externally</summary>
        public System.Action<CustomChain> OnChainModified;

        /// <summary>Event fired when the node selection changes</summary>
        public System.Action<int, int> OnSelectionChanged; // chainIndex, nodeIndex

        /// <summary>Fires the node modified notification</summary>
        /// <param name="modifiedNode">The node that was modified</param>
        public void NotifyNodeModified(CustomNode modifiedNode)
        {
            if (modifiedNode != null)
            {
                OnNodeModified?.Invoke(modifiedNode);
            }
        }

        /// <summary>Fires the chain modified notification</summary>
        /// <param name="modifiedChain">The chain that was modified</param>
        public void NotifyChainModified(CustomChain modifiedChain)
        {
            if (modifiedChain != null)
            {
                OnChainModified?.Invoke(modifiedChain);
            }
        }

        /// <summary>Fires the selection changed notification</summary>
        /// <param name="chainIndex">New selected chain index</param>
        /// <param name="nodeIndex">New selected node index</param>
        public void NotifySelectionChanged(int chainIndex, int nodeIndex)
        {
            OnSelectionChanged?.Invoke(chainIndex, nodeIndex);
        }

        #endregion
    }
}
#endif
