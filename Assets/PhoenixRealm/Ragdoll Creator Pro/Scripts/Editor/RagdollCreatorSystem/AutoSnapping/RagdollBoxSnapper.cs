#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PhoenixRealm.RagdollCreatorPro
{
    public static class RagdollBoxSnapper
    {
        #region Constants

        private const float MIN_CHAIN_LENGTH = 0.001f;
        private const float ALIGNMENT_TOLERANCE = 0.0001f;

        #endregion

        #region Box Alignment

        /// <summary>Aligns a box node to touch the face of a previous box node</summary>
        public static void AlignBoxToBox(CustomNode currentNode, CustomNode previousNode)
        {
            var currentTransform = currentNode.Transform;
            var previousTransform = previousNode.Transform;

            // Get world positions
            Vector3 previousWorldPos = previousTransform.position + previousTransform.TransformVector(previousNode.LocalOffset);
            Vector3 currentWorldPos = currentTransform.position;

            // Use bone direction
            Vector3 boneDirection = RagdollChainSnapper.GetBoneDirection(previousTransform, currentTransform);

            // Get box properties using bone rotation
            Vector3 previousSize = GetWorldBoxSize(previousNode);
            Vector3 currentSize = GetWorldBoxSize(currentNode);

            Matrix4x4 previousMatrix = GetWorldBoxMatrix(previousNode);
            Matrix4x4 currentMatrix = GetWorldBoxMatrix(currentNode);

            // Find the best face of previous box aligned with bone direction
            Vector3 previousConnectionPoint = GetBoxFaceCenter(previousWorldPos, previousMatrix, previousSize, boneDirection);

            // Find the best face of current box aligned with opposite bone direction
            Vector3 currentConnectionFace = GetBoxFaceCenter(currentWorldPos, currentMatrix, currentSize, -boneDirection);
            Vector3 currentFaceOffset = currentConnectionFace - currentWorldPos;

            // Calculate target center position
            Vector3 targetWorldCenter = previousConnectionPoint - currentFaceOffset;

            // Convert to local offset
            Vector3 worldDelta = targetWorldCenter - currentTransform.position;
            Vector3 localDelta = currentTransform.InverseTransformVector(worldDelta);

            currentNode.LocalOffset = localDelta;
        }

        /// <summary>Aligns a box node to touch a capsule node</summary>
        public static void AlignBoxToCapsule(CustomNode currentNode, CustomNode previousNode)
        {
            var currentTransform = currentNode.Transform;
            var previousTransform = previousNode.Transform;

            Vector3 previousWorldPos = previousTransform.position + previousTransform.TransformVector(previousNode.LocalOffset);
            Vector3 currentWorldPos = currentTransform.position;
            Vector3 boneDirection = RagdollChainSnapper.GetBoneDirection(previousTransform, currentTransform);

            // Get capsule tip using capsule direction
            Vector3 previousAxis = previousNode.GetCapsuleWorldAxis();
            float previousHalfHeight = RagdollCapsuleSnapper.GetWorldCapsuleHalfHeight(previousNode);
            float previousRadius = RagdollCapsuleSnapper.GetWorldCapsuleRadius(previousNode);

            Vector3 previousTip1 = previousWorldPos + previousAxis * (previousHalfHeight + previousRadius);
            Vector3 previousTip2 = previousWorldPos - previousAxis * (previousHalfHeight + previousRadius);

            float dot1 = Vector3.Dot((previousTip1 - previousWorldPos).normalized, boneDirection);
            float dot2 = Vector3.Dot((previousTip2 - previousWorldPos).normalized, boneDirection);
            Vector3 previousConnectionTip = dot1 > dot2 ? previousTip1 : previousTip2;

            // Get box face using bone rotation
            Vector3 currentSize = GetWorldBoxSize(currentNode);
            Matrix4x4 currentMatrix = GetWorldBoxMatrix(currentNode);
            Vector3 currentConnectionFace = GetBoxFaceCenter(currentWorldPos, currentMatrix, currentSize, -boneDirection);

            Vector3 currentFaceOffset = currentConnectionFace - currentWorldPos;
            Vector3 targetWorldCenter = previousConnectionTip - currentFaceOffset;

            Vector3 worldDelta = targetWorldCenter - currentTransform.position;
            Vector3 localDelta = currentTransform.InverseTransformVector(worldDelta);

            currentNode.LocalOffset = localDelta;
        }

        #endregion

        #region Box Properties

        /// <summary>Gets the world-space size of a box node</summary>
        public static Vector3 GetWorldBoxSize(CustomNode node)
        {
            if (!IsValidBoxNode(node))
                return Vector3.one;

            var transform = node.Transform;
            Vector3 localSize = node.ColliderSize;
            Vector3 worldSize = new Vector3(
                localSize.x * transform.lossyScale.x,
                localSize.y * transform.lossyScale.y,
                localSize.z * transform.lossyScale.z
            );

            return worldSize;
        }

        /// <summary>Gets the world transformation matrix for a box node using bone rotation</summary>
        public static Matrix4x4 GetWorldBoxMatrix(CustomNode node)
        {
            if (!IsValidBoxNode(node))
                return Matrix4x4.identity;

            var transform = node.Transform;
            var effectiveRotation = node.GetWorldRotation(); // Uses bone rotation directly
            var worldPos = transform.position + transform.TransformVector(node.LocalOffset);

            return Matrix4x4.TRS(worldPos, effectiveRotation, Vector3.one);
        }

        /// <summary>Gets the center point of the box face most aligned with the given direction</summary>
        public static Vector3 GetBoxFaceCenter(Vector3 boxCenter, Matrix4x4 boxMatrix, Vector3 boxSize, Vector3 direction)
        {
            // Box face normals in local space
            Vector3[] faceNormals = {
                Vector3.right,   // +X face
                Vector3.left,    // -X face
                Vector3.up,      // +Y face
                Vector3.down,    // -Y face
                Vector3.forward, // +Z face
                Vector3.back     // -Z face
            };

            // Face offsets from center
            Vector3[] faceOffsets = {
                Vector3.right * boxSize.x * 0.5f,
                Vector3.left * boxSize.x * 0.5f,
                Vector3.up * boxSize.y * 0.5f,
                Vector3.down * boxSize.y * 0.5f,
                Vector3.forward * boxSize.z * 0.5f,
                Vector3.back * boxSize.z * 0.5f
            };

            // Find the face most aligned with the direction
            float bestDot = -1f;
            int bestFaceIndex = 0;

            for (int i = 0; i < faceNormals.Length; i++)
            {
                Vector3 worldNormal = boxMatrix.MultiplyVector(faceNormals[i]).normalized;
                float dot = Vector3.Dot(worldNormal, direction.normalized);
                if (dot > bestDot)
                {
                    bestDot = dot;
                    bestFaceIndex = i;
                }
            }

            // Calculate world position of the face center
            Vector3 localFaceCenter = faceOffsets[bestFaceIndex];
            Vector3 worldFaceCenter = boxMatrix.MultiplyPoint3x4(localFaceCenter);

            return worldFaceCenter;
        }

        #endregion

        #region Box Auto-Sizing

        /// <summary>Calculates optimal box size based on distance to next node</summary>
        public static Vector3 CalculateOptimalBoxSize(CustomNode node, CustomNode nextNode)
        {
            var transform = node.Transform;
            Vector3 currentSize = node.ColliderSize;

            // Use distance to next node if available
            if (nextNode?.Transform != null)
            {
                float distanceToNext = Vector3.Distance(transform.position, nextNode.Transform.position);
                Vector3 boneDirection = RagdollChainSnapper.GetBoneDirection(transform, nextNode.Transform);

                // Use bone rotation to determine which axis is most aligned with bone direction
                Vector3 worldRight = node.GetWorldRightAxis();
                Vector3 worldUp = node.GetWorldUpAxis();
                Vector3 worldForward = node.GetWorldForwardAxis();

                // Find the most aligned axis
                float dotX = Mathf.Abs(Vector3.Dot(worldRight, boneDirection));
                float dotY = Mathf.Abs(Vector3.Dot(worldUp, boneDirection));
                float dotZ = Mathf.Abs(Vector3.Dot(worldForward, boneDirection));

                Vector3 optimalSize = currentSize;

                if (dotX > dotY && dotX > dotZ)
                {
                    // X-axis is most aligned
                    optimalSize.x = distanceToNext * 0.9f;
                }
                else if (dotY > dotZ)
                {
                    // Y-axis is most aligned
                    optimalSize.y = distanceToNext * 0.9f;
                }
                else
                {
                    // Z-axis is most aligned
                    optimalSize.z = distanceToNext * 0.9f;
                }

                return optimalSize;
            }

            // Fallback: keep current size
            return currentSize;
        }

        #endregion

        #region Validation

        /// <summary>Checks if a node is valid for box alignment operations</summary>
        public static bool IsValidBoxNode(CustomNode node)
        {
            return node != null &&
                   node.Transform != null &&
                   node.ColliderType == ColliderType.Box &&
                   node.ColliderSize.x > 0f &&
                   node.ColliderSize.y > 0f &&
                   node.ColliderSize.z > 0f;
        }

        /// <summary>Gets all valid box nodes from a list</summary>
        public static List<CustomNode> GetValidBoxNodes(List<CustomNode> nodes)
        {
            var validNodes = new List<CustomNode>();

            foreach (var node in nodes)
            {
                if (IsValidBoxNode(node))
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
