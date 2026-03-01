#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PhoenixRealm.RagdollCreatorPro
{
    public static class RagdollChainSnapper
    {
        #region Constants

        private const float MIN_CHAIN_LENGTH = 0.001f;
        private const float ALIGNMENT_TOLERANCE = 0.0001f;

        #endregion

        #region Public API - Chain Operations

        /// <summary>Aligns all colliders in a chain end-to-end without gaps (supports capsules and boxes)</summary>
        public static void AlignColliderChain(List<CustomNode> nodes)
        {
            if (nodes == null || nodes.Count < 2)
            {
                Debug.LogWarning("Chain must have at least 2 nodes for alignment");
                return;
            }

            Undo.SetCurrentGroupName("Align Collider Chain");
            int undoGroup = Undo.GetCurrentGroup();

            try
            {
                // Filter only valid collider nodes (capsules and boxes)
                var validNodes = GetValidColliderNodes(nodes);
                if (validNodes.Count < 2)
                {
                    Debug.LogWarning("Chain must have at least 2 valid collider nodes for alignment");
                    return;
                }

                Debug.Log($"Starting alignment of {validNodes.Count} collider nodes");

                // Reset ONLY child nodes' LocalOffsets to zero (preserve parent/root node)
                for (int i = 1; i < validNodes.Count; i++)
                {
                    validNodes[i].LocalOffset = Vector3.zero;
                }

                Debug.Log($"Parent node (index 0) LocalOffset preserved: {validNodes[0].LocalOffset}");

                // Align child nodes sequentially from second node onwards
                for (int i = 1; i < validNodes.Count; i++)
                {
                    var currentNode = validNodes[i];
                    var previousNode = validNodes[i - 1];

                    AlignNodeToPrevious(currentNode, previousNode);

                    Debug.Log($"Child Node {i}: LocalOffset = {currentNode.LocalOffset}");
                }

                Debug.Log($"Successfully aligned {validNodes.Count - 1} child collider nodes (parent unchanged)");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error aligning collider chain: {e.Message}");
                Undo.RevertAllDownToGroup(undoGroup);
            }
        }

        /// <summary>Legacy method for backward compatibility - redirects to AlignColliderChain</summary>
        public static void AlignCapsuleChain(List<CustomNode> nodes)
        {
            AlignColliderChain(nodes);
        }

        /// <summary>Auto-sizes colliders in a chain based on bone distances</summary>
        public static void AutoSizeColliderChain(List<CustomNode> nodes)
        {
            if (nodes == null || nodes.Count < 2)
                return;

            Undo.SetCurrentGroupName("Auto-Size Collider Chain");

            var validNodes = GetValidColliderNodes(nodes);

            for (int i = 0; i < validNodes.Count; i++)
            {
                var node = validNodes[i];
                var nextNode = i < validNodes.Count - 1 ? validNodes[i + 1] : null;

                if (node.ColliderType == ColliderType.Capsule)
                {
                    float optimalHeight = RagdollCapsuleSnapper.CalculateOptimalCapsuleHeight(node, nextNode);
                    if (optimalHeight > MIN_CHAIN_LENGTH)
                    {
                        node.ColliderHeight = optimalHeight;
                    }
                }
                else if (node.ColliderType == ColliderType.Box)
                {
                    Vector3 optimalSize = RagdollBoxSnapper.CalculateOptimalBoxSize(node, nextNode);
                    if (optimalSize.sqrMagnitude > MIN_CHAIN_LENGTH)
                    {
                        node.ColliderSize = optimalSize;
                    }
                }
            }

            Debug.Log($"Auto-sized {validNodes.Count} collider nodes");
        }

        #endregion

        #region Child Maintenance

        /// <summary>Maintains alignment of children when a specific node is manually edited</summary>
        public static void MaintainChildrenOfNode(List<CustomNode> nodes, CustomNode editedNode)
        {
            if (nodes == null || editedNode == null || !IsValidColliderNode(editedNode))
            {
                Debug.LogWarning("Invalid parameters for maintaining children");
                return;
            }

            Undo.SetCurrentGroupName("Maintain Children Alignment");
            int undoGroup = Undo.GetCurrentGroup();

            try
            {
                var validNodes = GetValidColliderNodes(nodes);
                int editedNodeIndex = validNodes.IndexOf(editedNode);

                if (editedNodeIndex == -1)
                {
                    Debug.LogWarning($"Edited node '{editedNode.Transform.name}' not found in chain");
                    return;
                }

                // Align all children sequentially from the edited node onwards
                for (int i = editedNodeIndex + 1; i < validNodes.Count; i++)
                {
                    var currentNode = validNodes[i];
                    var previousNode = validNodes[i - 1];

                    // Reset child's local offset before alignment
                    currentNode.LocalOffset = Vector3.zero;
                    AlignNodeToPrevious(currentNode, previousNode);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error maintaining children: {e.Message}");
                Undo.RevertAllDownToGroup(undoGroup);
            }
        }

        /// <summary>Maintains alignment of children for the currently selected transform in Unity</summary>
        public static void MaintainChildrenOfSelectedTransform(List<CustomChain> chains)
        {
            if (chains == null || chains.Count == 0)
            {
                Debug.LogWarning("No chains provided for maintaining children");
                return;
            }

            var selectedTransform = Selection.activeTransform;
            if (selectedTransform == null)
            {
                Debug.LogWarning("No transform selected in Unity hierarchy");
                return;
            }

            // Find the node and chain that contains the selected transform
            CustomNode foundNode = null;
            CustomChain foundChain = null;

            foreach (var chain in chains)
            {
                foundNode = FindNodeByTransform(chain.Nodes, selectedTransform);
                if (foundNode != null)
                {
                    foundChain = chain;
                    break;
                }
            }

            if (foundNode == null)
            {
                Debug.LogWarning($"Selected transform '{selectedTransform.name}' not found in any chain");
                return;
            }

            Debug.Log($"Found selected node '{foundNode.Transform.name}' in chain '{foundChain.ChainName}'");
            MaintainChildrenOfNode(foundChain.Nodes, foundNode);
        }

        /// <summary>Maintains alignment of children for a specific transform in the provided chains</summary>
        public static void MaintainChildrenOfTransform(List<CustomChain> chains, Transform targetTransform)
        {
            if (chains == null || targetTransform == null)
            {
                Debug.LogWarning("Invalid parameters for maintaining children of transform");
                return;
            }

            // Find the node and chain that contains the target transform
            CustomNode foundNode = null;
            CustomChain foundChain = null;

            foreach (var chain in chains)
            {
                foundNode = FindNodeByTransform(chain.Nodes, targetTransform);
                if (foundNode != null)
                {
                    foundChain = chain;
                    break;
                }
            }

            if (foundNode == null)
            {
                Debug.LogWarning($"Transform '{targetTransform.name}' not found in any chain");
                return;
            }

            Debug.Log($"Maintaining children of transform '{targetTransform.name}' in chain '{foundChain.ChainName}'");
            MaintainChildrenOfNode(foundChain.Nodes, foundNode);
        }

        #endregion

        #region Core Alignment Logic

        /// <summary>Aligns a single node to touch the previous node (supports both capsules and boxes)</summary>
        public static void AlignNodeToPrevious(CustomNode currentNode, CustomNode previousNode)
        {
            if (!IsValidColliderNode(currentNode) || !IsValidColliderNode(previousNode))
                return;

            // Delegate to specialized handlers based on node types
            if (RagdollCapsuleSnapper.IsValidCapsuleNode(currentNode) && RagdollCapsuleSnapper.IsValidCapsuleNode(previousNode))
            {
                RagdollCapsuleSnapper.AlignCapsuleToCapsule(currentNode, previousNode);
            }
            else if (RagdollBoxSnapper.IsValidBoxNode(currentNode) && RagdollBoxSnapper.IsValidBoxNode(previousNode))
            {
                RagdollBoxSnapper.AlignBoxToBox(currentNode, previousNode);
            }
            else if (RagdollCapsuleSnapper.IsValidCapsuleNode(currentNode) && RagdollBoxSnapper.IsValidBoxNode(previousNode))
            {
                RagdollCapsuleSnapper.AlignCapsuleToBox(currentNode, previousNode);
            }
            else if (RagdollBoxSnapper.IsValidBoxNode(currentNode) && RagdollCapsuleSnapper.IsValidCapsuleNode(previousNode))
            {
                RagdollBoxSnapper.AlignBoxToCapsule(currentNode, previousNode);
            }
            else
            {
                Debug.LogWarning($"Unsupported alignment between {currentNode.ColliderType} and {previousNode.ColliderType}");
            }
        }

        #endregion

        #region Shared Utilities

        /// <summary>Gets all valid collider nodes (capsules and boxes) from a list</summary>
        public static List<CustomNode> GetValidColliderNodes(List<CustomNode> nodes)
        {
            var validNodes = new List<CustomNode>();

            foreach (var node in nodes)
            {
                if (IsValidColliderNode(node))
                {
                    validNodes.Add(node);
                }
            }

            return validNodes;
        }

        /// <summary>Finds a node by its transform reference in a list of nodes</summary>
        public static CustomNode FindNodeByTransform(List<CustomNode> nodes, Transform targetTransform)
        {
            if (nodes == null || targetTransform == null)
                return null;

            foreach (var node in nodes)
            {
                if (node.Transform == targetTransform)
                {
                    return node;
                }
            }

            return null;
        }

        /// <summary>Gets all child nodes that come after a specific node in a chain</summary>
        public static List<CustomNode> GetChildNodesAfter(List<CustomNode> nodes, CustomNode parentNode)
        {
            var childNodes = new List<CustomNode>();

            if (nodes == null || parentNode == null)
                return childNodes;

            var validNodes = GetValidColliderNodes(nodes);
            int parentIndex = validNodes.IndexOf(parentNode);

            if (parentIndex == -1 || parentIndex >= validNodes.Count - 1)
                return childNodes;

            // Get all nodes after the parent
            for (int i = parentIndex + 1; i < validNodes.Count; i++)
            {
                childNodes.Add(validNodes[i]);
            }

            return childNodes;
        }

        /// <summary>Gets the actual bone direction from one transform to another in the hierarchy</summary>
        public static Vector3 GetBoneDirection(Transform fromBone, Transform toBone)
        {
            // Method 1: Use direct bone-to-bone direction (most reliable)
            Vector3 directDirection = (toBone.position - fromBone.position).normalized;

            if (directDirection.sqrMagnitude > ALIGNMENT_TOLERANCE)
            {
                return directDirection;
            }

            // Method 2: Check if toBone is a child of fromBone
            if (toBone.parent == fromBone)
            {
                return directDirection.sqrMagnitude > ALIGNMENT_TOLERANCE ? directDirection : fromBone.forward;
            }

            // Method 3: Use fromBone's direction to its primary child
            Vector3 primaryChildDirection = GetBonePrimaryDirection(fromBone);
            if (primaryChildDirection.sqrMagnitude > ALIGNMENT_TOLERANCE)
            {
                return primaryChildDirection;
            }

            // Fallback: Use the bone's forward direction
            return fromBone.forward;
        }

        /// <summary>Gets the primary direction of a bone based on its longest child</summary>
        public static Vector3 GetBonePrimaryDirection(Transform bone)
        {
            if (bone.childCount == 0)
                return Vector3.zero;

            // Find the child that's furthest away (primary bone direction)
            Transform primaryChild = null;
            float longestDistance = 0f;

            for (int i = 0; i < bone.childCount; i++)
            {
                var child = bone.GetChild(i);
                float distance = Vector3.Distance(bone.position, child.position);
                if (distance > longestDistance)
                {
                    longestDistance = distance;
                    primaryChild = child;
                }
            }

            if (primaryChild != null && longestDistance > ALIGNMENT_TOLERANCE)
            {
                return (primaryChild.position - bone.position).normalized;
            }

            return Vector3.zero;
        }

        #endregion

        #region Validation

        /// <summary>Checks if a node is valid for collider alignment operations (capsule or box)</summary>
        public static bool IsValidColliderNode(CustomNode node)
        {
            return RagdollCapsuleSnapper.IsValidCapsuleNode(node) || RagdollBoxSnapper.IsValidBoxNode(node);
        }

        /// <summary>Legacy validation - checks if a node is valid for capsule operations</summary>
        public static bool IsValidCapsuleNode(CustomNode node)
        {
            return RagdollCapsuleSnapper.IsValidCapsuleNode(node);
        }

        #endregion
    }
}
#endif
