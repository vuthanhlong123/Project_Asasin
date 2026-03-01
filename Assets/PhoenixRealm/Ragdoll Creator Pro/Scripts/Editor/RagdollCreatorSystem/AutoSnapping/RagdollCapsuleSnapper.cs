#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PhoenixRealm.RagdollCreatorPro
{
    public static class RagdollCapsuleSnapper
    {
        #region Constants

        private const float MIN_CHAIN_LENGTH = 0.001f;
        private const float ALIGNMENT_TOLERANCE = 0.0001f;

        #endregion

        #region Capsule Alignment

        /// <summary>Aligns a capsule node to touch the tip of a previous capsule node</summary>
        public static void AlignCapsuleToCapsule(CustomNode currentNode, CustomNode previousNode)
        {
            var currentTransform = currentNode.Transform;
            var previousTransform = previousNode.Transform;

            // Get world positions (previous node includes its current LocalOffset)
            Vector3 previousWorldPos = previousTransform.position + previousTransform.TransformVector(previousNode.LocalOffset);
            Vector3 currentWorldPos = currentTransform.position;

            // Use ACTUAL bone direction from the previous bone to current bone
            Vector3 boneDirection = RagdollChainSnapper.GetBoneDirection(previousTransform, currentTransform);

            // Get previous node's world properties using capsule direction
            Vector3 previousAxis = previousNode.GetCapsuleWorldAxis();
            float previousHalfHeight = GetWorldCapsuleHalfHeight(previousNode);
            float previousRadius = GetWorldCapsuleRadius(previousNode);

            // Get current node's world properties using capsule direction
            Vector3 currentAxis = currentNode.GetCapsuleWorldAxis();
            float currentHalfHeight = GetWorldCapsuleHalfHeight(currentNode);
            float currentRadius = GetWorldCapsuleRadius(currentNode);

            // Calculate ACTUAL tip positions including hemisphere radius
            Vector3 previousTip1 = previousWorldPos + previousAxis * (previousHalfHeight + previousRadius);
            Vector3 previousTip2 = previousWorldPos - previousAxis * (previousHalfHeight + previousRadius);

            // Choose the tip that's more aligned with the bone direction
            float dot1 = Vector3.Dot((previousTip1 - previousWorldPos).normalized, boneDirection);
            float dot2 = Vector3.Dot((previousTip2 - previousWorldPos).normalized, boneDirection);
            Vector3 previousConnectionTip = dot1 > dot2 ? previousTip1 : previousTip2;

            // For current capsule, choose the tip that's more aligned with the OPPOSITE bone direction
            Vector3 currentTip1 = currentWorldPos + currentAxis * (currentHalfHeight + currentRadius);
            Vector3 currentTip2 = currentWorldPos - currentAxis * (currentHalfHeight + currentRadius);

            float currentDot1 = Vector3.Dot((currentTip1 - currentWorldPos).normalized, -boneDirection);
            float currentDot2 = Vector3.Dot((currentTip2 - currentWorldPos).normalized, -boneDirection);
            Vector3 currentConnectionTip = currentDot1 > currentDot2 ? currentTip1 : currentTip2;

            // Calculate the world position where current center should be to make tips touch
            Vector3 currentTipOffset = currentConnectionTip - currentWorldPos;
            Vector3 targetWorldCenter = previousConnectionTip - currentTipOffset;

            // Convert the required world position to local offset
            Vector3 worldDelta = targetWorldCenter - currentTransform.position;
            Vector3 localDelta = currentTransform.InverseTransformVector(worldDelta);

            // Update the current node's local offset
            currentNode.LocalOffset = localDelta;
        }

        /// <summary>Aligns a capsule node to touch a box node</summary>
        public static void AlignCapsuleToBox(CustomNode currentNode, CustomNode previousNode)
        {
            var currentTransform = currentNode.Transform;
            var previousTransform = previousNode.Transform;

            Vector3 previousWorldPos = previousTransform.position + previousTransform.TransformVector(previousNode.LocalOffset);
            Vector3 currentWorldPos = currentTransform.position;
            Vector3 boneDirection = RagdollChainSnapper.GetBoneDirection(previousTransform, currentTransform);

            // Get box face center using bone rotation
            Vector3 previousSize = RagdollBoxSnapper.GetWorldBoxSize(previousNode);
            Matrix4x4 previousMatrix = RagdollBoxSnapper.GetWorldBoxMatrix(previousNode);
            Vector3 previousConnectionPoint = RagdollBoxSnapper.GetBoxFaceCenter(previousWorldPos, previousMatrix, previousSize, boneDirection);

            // Get capsule tip using capsule direction
            Vector3 currentAxis = currentNode.GetCapsuleWorldAxis();
            float currentHalfHeight = GetWorldCapsuleHalfHeight(currentNode);
            float currentRadius = GetWorldCapsuleRadius(currentNode);

            Vector3 currentTip1 = currentWorldPos + currentAxis * (currentHalfHeight + currentRadius);
            Vector3 currentTip2 = currentWorldPos - currentAxis * (currentHalfHeight + currentRadius);

            // Choose the tip aligned with opposite bone direction
            float dot1 = Vector3.Dot((currentTip1 - currentWorldPos).normalized, -boneDirection);
            float dot2 = Vector3.Dot((currentTip2 - currentWorldPos).normalized, -boneDirection);
            Vector3 currentConnectionTip = dot1 > dot2 ? currentTip1 : currentTip2;

            Vector3 currentTipOffset = currentConnectionTip - currentWorldPos;
            Vector3 targetWorldCenter = previousConnectionPoint - currentTipOffset;

            Vector3 worldDelta = targetWorldCenter - currentTransform.position;
            Vector3 localDelta = currentTransform.InverseTransformVector(worldDelta);

            currentNode.LocalOffset = localDelta;
        }

        #endregion

        #region Capsule Properties

        /// <summary>Gets the world-space half-height of a capsule node (cylindrical portion only)</summary>
        public static float GetWorldCapsuleHalfHeight(CustomNode node)
        {
            if (!IsValidCapsuleNode(node))
                return 0f;

            var transform = node.Transform;

            // Scale is applied based on the capsule direction
            float scale = 1f;
            switch (node.CapsuleDirection)
            {
                case CapsuleDirection.X:
                    scale = transform.lossyScale.x;
                    break;
                case CapsuleDirection.Y:
                    scale = transform.lossyScale.y;
                    break;
                case CapsuleDirection.Z:
                    scale = transform.lossyScale.z;
                    break;
            }

            float worldHeight = node.ColliderHeight * scale;
            return worldHeight * 0.5f;
        }

        /// <summary>Gets the world-space radius of a capsule node</summary>
        public static float GetWorldCapsuleRadius(CustomNode node)
        {
            if (!IsValidCapsuleNode(node))
                return 0f;

            var transform = node.Transform;

            // For capsules, radius is affected by the two axes perpendicular to the height axis
            float radiusScale = 1f;
            switch (node.CapsuleDirection)
            {
                case CapsuleDirection.X:
                    // Height is X, so radius uses Y and Z
                    radiusScale = Mathf.Max(transform.lossyScale.y, transform.lossyScale.z);
                    break;
                case CapsuleDirection.Y:
                    // Height is Y, so radius uses X and Z
                    radiusScale = Mathf.Max(transform.lossyScale.x, transform.lossyScale.z);
                    break;
                case CapsuleDirection.Z:
                    // Height is Z, so radius uses X and Y
                    radiusScale = Mathf.Max(transform.lossyScale.x, transform.lossyScale.y);
                    break;
            }

            return node.ColliderRadius * radiusScale;
        }

        #endregion

        #region Capsule Auto-Sizing

        /// <summary>Calculates optimal capsule height based on distance to next node</summary>
        public static float CalculateOptimalCapsuleHeight(CustomNode node, CustomNode nextNode)
        {
            var transform = node.Transform;

            // Use distance to next node if available
            if (nextNode?.Transform != null)
            {
                float distanceToNext = Vector3.Distance(transform.position, nextNode.Transform.position);

                // Account for both radii when calculating optimal height
                float nodeRadius = GetWorldCapsuleRadius(node);
                float nextRadius = IsValidCapsuleNode(nextNode) ? GetWorldCapsuleRadius(nextNode) : 0f;

                // Subtract the radii to get the cylindrical height needed
                float optimalHeight = distanceToNext - nodeRadius - nextRadius;
                return Mathf.Max(optimalHeight * 0.9f, nodeRadius * 2f); // Minimum height = 2 * radius
            }

            // Fallback: use distance to primary child
            if (transform.childCount > 0)
            {
                Transform longestChild = null;
                float longestDistance = 0f;

                for (int i = 0; i < transform.childCount; i++)
                {
                    var child = transform.GetChild(i);
                    float distance = Vector3.Distance(transform.position, child.position);
                    if (distance > longestDistance)
                    {
                        longestDistance = distance;
                        longestChild = child;
                    }
                }

                if (longestChild != null)
                {
                    float nodeRadius = GetWorldCapsuleRadius(node);
                    float optimalHeight = longestDistance - nodeRadius;
                    return Mathf.Max(optimalHeight * 0.9f, nodeRadius * 2f);
                }
            }

            // Keep current height as final fallback
            return node.ColliderHeight;
        }

        #endregion

        #region Validation

        /// <summary>Checks if a node is valid for capsule alignment operations</summary>
        public static bool IsValidCapsuleNode(CustomNode node)
        {
            return node != null &&
                   node.Transform != null &&
                   node.ColliderType == ColliderType.Capsule &&
                   node.ColliderRadius > 0f &&
                   node.ColliderHeight > 0f;
        }

        /// <summary>Gets all valid capsule nodes from a list</summary>
        public static List<CustomNode> GetValidCapsuleNodes(List<CustomNode> nodes)
        {
            var validNodes = new List<CustomNode>();

            foreach (var node in nodes)
            {
                if (IsValidCapsuleNode(node))
                {
                    validNodes.Add(node);
                }
            }

            return validNodes;
        }

        #endregion
    }
}
#endif
