#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PhoenixRealm.RagdollCreatorPro.Editor
{
    public static class RagdollMakerBake
    {
        public static void BakeRagdoll(List<CustomChain> chains)
        {
            Undo.SetCurrentGroupName("Bake Ragdoll");
            int undoGroup = Undo.GetCurrentGroup();

            try
            {
                foreach (var chain in chains)
                {
                    BakeChain(chain);
                }

                Debug.Log($"Ragdoll baked successfully from {chains.Count} chains");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error baking ragdoll: {e.Message}");
                Undo.RevertAllDownToGroup(undoGroup);
            }
        }

        public static void BakeChain(CustomChain chain)
        {
            for (int i = 0; i < chain.Nodes.Count; i++)
            {
                var node = chain.Nodes[i];
                if (node.Transform == null) continue;

                BakeNode(node, i == 0);

                if (i == 0)
                {
                    if (chain.ParentNode != null && chain.ParentNode.Transform != null)
                    {
                        BakeJoint(node, chain.ParentNode);
                    }
                }
                else
                {
                    var parentNode = chain.Nodes[i - 1];
                    BakeJoint(node, parentNode);
                }
            }

            DisableParentChildCollisions(chain);

            if (chain.ParentNode != null && chain.Nodes.Count > 0)
            {
                DisableCollisionBetweenNodes(chain.Nodes[0], chain.ParentNode);
            }
        }

        public static void BakeNode(CustomNode node, bool isRoot)
        {
            var transform = node.Transform;

            var rigidbody = transform.GetComponent<Rigidbody>();
            if (rigidbody == null)
            {
                rigidbody = Undo.AddComponent<Rigidbody>(transform.gameObject);
            }
            else
            {
                Undo.RecordObject(rigidbody, "Modify Rigidbody");
            }

            rigidbody.mass = node.MassOverride;
            rigidbody.isKinematic = false;
            rigidbody.useGravity = true;

            AddColliderToNode(node);
        }

        public static void AddColliderToNode(CustomNode node)
        {
            var transform = node.Transform;

            var existingColliders = transform.GetComponents<Collider>();
            foreach (var collider in existingColliders)
            {
                Undo.DestroyObjectImmediate(collider);
            }

            Vector3 center = node.LocalOffset;

            switch (node.ColliderType)
            {
                case ColliderType.Sphere:
                    var sphereCollider = Undo.AddComponent<SphereCollider>(transform.gameObject);
                    sphereCollider.radius = node.ColliderRadius;
                    sphereCollider.center = center;
                    break;

                case ColliderType.Capsule:
                    var capsuleCollider = Undo.AddComponent<CapsuleCollider>(transform.gameObject);
                    capsuleCollider.radius = node.ColliderRadius;

                    float totalHeight = node.ColliderHeight + (node.ColliderRadius * 2f);
                    capsuleCollider.height = Mathf.Max(node.ColliderRadius * 2f, totalHeight);

                    capsuleCollider.direction = (int)node.CapsuleDirection;
                    capsuleCollider.center = center;
                    break;

                case ColliderType.Box:
                    var boxCollider = Undo.AddComponent<BoxCollider>(transform.gameObject);
                    boxCollider.size = node.ColliderSize;
                    boxCollider.center = center;
                    break;
            }
        }

        public static void BakeJoint(CustomNode node, CustomNode parentNode)
        {
            var transform = node.Transform;
            var parentTransform = parentNode.Transform;

            if (parentTransform == null) return;

            var existingJoint = transform.GetComponent<CharacterJoint>();
            if (existingJoint != null)
            {
                Undo.DestroyObjectImmediate(existingJoint);
            }

            var joint = Undo.AddComponent<CharacterJoint>(transform.gameObject);

            var parentRigidbody = parentTransform.GetComponent<Rigidbody>();
            joint.connectedBody = parentRigidbody;

            joint.anchor = node.JointAnchorLocal;
            joint.connectedAnchor = node.JointConnectedAnchorLocal;

            joint.axis = node.JointAxisLocal;

            Vector3 swingAxis = Vector3.Cross(node.JointAxisLocal, Vector3.up);
            if (swingAxis.sqrMagnitude < 1e-6f)
                swingAxis = Vector3.Cross(node.JointAxisLocal, Vector3.forward);
            joint.swingAxis = swingAxis.normalized;

            joint.lowTwistLimit = new SoftJointLimit()
            {
                limit = node.JointLimits.lowTwistLimit.Limit,
                bounciness = node.JointLimits.lowTwistLimit.Bounciness,
                contactDistance = node.JointLimits.lowTwistLimit.ContactDistance,
            };
            joint.highTwistLimit = new SoftJointLimit()
            {
                limit = node.JointLimits.highTwistLimit.Limit,
                bounciness = node.JointLimits.highTwistLimit.Bounciness,
                contactDistance = node.JointLimits.highTwistLimit.ContactDistance,
            };
            joint.swing1Limit = new SoftJointLimit()
            {
                limit = node.JointLimits.swing1Limit.Limit,
                bounciness = node.JointLimits.swing1Limit.Bounciness,
                contactDistance = node.JointLimits.swing1Limit.ContactDistance,
            };
            joint.swing2Limit = new SoftJointLimit()
            {
                limit = node.JointLimits.swing2Limit.Limit,
                bounciness = node.JointLimits.swing2Limit.Bounciness,
                contactDistance = node.JointLimits.swing2Limit.ContactDistance,
            };

            joint.enableProjection = node.JointEnableProjection;
            joint.enablePreprocessing = node.JointEnablePreprocessing;

            if (node.JointStability)
            {
                joint.enablePreprocessing = false;
                joint.projectionDistance = 0.1f;
                joint.projectionAngle = 180f;
            }
        }

        public static void DisableParentChildCollisions(CustomChain chain)
        {
            for (int i = 1; i < chain.Nodes.Count; i++)
            {
                var node = chain.Nodes[i];
                var parentNode = chain.Nodes[i - 1];

                if (node.Transform == null || parentNode.Transform == null)
                    continue;

                var nodeCollider = node.Transform.GetComponent<Collider>();
                var parentCollider = parentNode.Transform.GetComponent<Collider>();

                if (nodeCollider != null && parentCollider != null)
                {
                    Physics.IgnoreCollision(nodeCollider, parentCollider, true);
                }
            }
        }

        private static void DisableCollisionBetweenNodes(CustomNode nodeA, CustomNode nodeB)
        {
            if (nodeA?.Transform == null || nodeB?.Transform == null) return;

            var colliderA = nodeA.Transform.GetComponent<Collider>();
            var colliderB = nodeB.Transform.GetComponent<Collider>();

            if (colliderA != null && colliderB != null)
            {
                Physics.IgnoreCollision(colliderA, colliderB, true);
            }
        }
    }
}
#endif
