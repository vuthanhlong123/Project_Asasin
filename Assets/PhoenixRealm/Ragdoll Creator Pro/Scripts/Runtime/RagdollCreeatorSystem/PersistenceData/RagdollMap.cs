using System.Collections.Generic;
using UnityEngine;

namespace PhoenixRealm.RagdollCreatorPro
{
    public class RagdollMap : MonoBehaviour
    {
        #region Vars + Properties

        [SerializeField] private List<ChainData> m_chains = new List<ChainData>();

        public List<ChainData> Chains
        {
            get => m_chains;
            set => m_chains = value ?? new List<ChainData>();
        }

        #endregion

        #region Unity Functions

        private void Awake()
        {
            if (m_chains == null)
                m_chains = new List<ChainData>();
        }

        #endregion

        #region Custom Functions

        public void PullFromChains(IList<CustomChain> chains)
        {
            m_chains.Clear();

            if (chains == null) return;

            foreach (var chain in chains)
            {
                if (chain == null) continue;

                var chainData = new ChainData
                {
                    ChainName = chain.ChainName,
                    ParentNodeTransform = chain.ParentNode?.Transform,
                    TotalMass = chain.TotalMass,

                    Nodes = new List<NodeData>()
                };

                if (chain.Nodes != null)
                {
                    foreach (var node in chain.Nodes)
                    {
                        if (node == null) continue;

                        var nodeData = NodeDataFromCustomNode(node);
                        chainData.Nodes.Add(nodeData);
                    }
                }

                m_chains.Add(chainData);
            }
        }

        public void PushToChains(IList<CustomChain> chains)
        {

            if (chains == null || m_chains == null) return;

            chains.Clear();

            foreach (var chainData in m_chains)
            {
                if (chainData == null) continue;

                var chain = new CustomChain(chainData.ChainName, chainData.TotalMass);

                if (chainData.ParentNodeTransform != null)
                {
                    chain.ParentNode = new CustomNode(chainData.ParentNodeTransform);
                }

                if (chainData.Nodes != null)
                {
                    foreach (var nodeData in chainData.Nodes)
                    {
                        if (nodeData == null) continue;

                        var node = CustomNodeFromNodeData(nodeData);
                        if (node != null)
                        {
                            chain.Nodes.Add(node);
                        }
                    }
                }

                chains.Add(chain);
            }
        }

        public void UpsertNode(CustomChain chain, CustomNode node)
        {
            if (chain == null || node == null) return;

            var chainData = m_chains.Find(c => c.ChainName == chain.ChainName);
            if (chainData == null)
            {
                chainData = new ChainData
                {
                    ChainName = chain.ChainName,
                    TotalMass = chain.TotalMass,
                    Nodes = new List<NodeData>()
                };
                m_chains.Add(chainData);
            }

            NodeData existingNodeData = null;
            if (node.Transform != null)
            {
                existingNodeData = chainData.Nodes.Find(n => n.Bone == node.Transform);
            }

            if (existingNodeData == null && !string.IsNullOrEmpty(node.NodeName))
            {
                existingNodeData = chainData.Nodes.Find(n => n.NodeName == node.NodeName);
            }

            var nodeData = NodeDataFromCustomNode(node);

            if (existingNodeData != null)
            {
                int index = chainData.Nodes.IndexOf(existingNodeData);
                chainData.Nodes[index] = nodeData;
            }
            else
            {
                chainData.Nodes.Add(nodeData);
            }
        }

        #endregion

        #region Getters

        private NodeData NodeDataFromCustomNode(CustomNode node)
        {
            var jointLimits = node.JointLimits;

            return new NodeData
            {
                NodeName = node.NodeName,
                Bone = node.Transform,
                ColliderType = node.ColliderType,
                ColliderSize = node.ColliderSize,
                ColliderRadius = node.ColliderRadius,
                ColliderHeight = node.ColliderHeight,
                CapsuleDirection = node.CapsuleDirection,
                LocalOffset = node.LocalOffset,
                MassOverride = node.MassOverride,
                JointStability = node.JointStability,
                JointAnchorLocal = node.JointAnchorLocal,
                JointConnectedAnchorLocal = node.JointConnectedAnchorLocal,
                JointAxisLocal = node.JointAxisLocal,
                JointLimits = jointLimits.Clone(),
                JointEnableProjection = node.JointEnableProjection,
                JointEnablePreprocessing = node.JointEnablePreprocessing
            };
        }

        private CustomNode CustomNodeFromNodeData(NodeData data)
        {
            var node = new CustomNode(data.Bone)
            {
                NodeName = data.NodeName,
                ColliderType = data.ColliderType,
                ColliderSize = data.ColliderSize,
                ColliderRadius = data.ColliderRadius,
                ColliderHeight = data.ColliderHeight,
                CapsuleDirection = data.CapsuleDirection,
                LocalOffset = data.LocalOffset,
                MassOverride = data.MassOverride,
                JointStability = data.JointStability,
                JointAnchorLocal = data.JointAnchorLocal,
                JointConnectedAnchorLocal = data.JointConnectedAnchorLocal,
                JointAxisLocal = data.JointAxisLocal,
                JointEnableProjection = data.JointEnableProjection,
                JointEnablePreprocessing = data.JointEnablePreprocessing
            };

            if (data.JointLimits == null)
            {
                node.JointLimits = CustomNodeJointLimits.Default();
            }
            else
            {
                node.JointLimits = data.JointLimits.Clone();
            }

            return node;
        }

        #endregion

        [System.Serializable]
        public class ChainData
        {
            public string ChainName;
            public float TotalMass;
            public List<NodeData> Nodes;

            public Transform ParentNodeTransform;
        }

        [System.Serializable]
        public class NodeData
        {
            public string NodeName;
            public Transform Bone;
            public ColliderType ColliderType;
            public Vector3 ColliderSize;
            public float ColliderRadius;
            public float ColliderHeight;
            public CapsuleDirection CapsuleDirection;
            public Vector3 LocalOffset;
            public float MassOverride;
            public bool JointStability;
            public Vector3 JointAnchorLocal;
            public Vector3 JointConnectedAnchorLocal;
            public Vector3 JointAxisLocal;
            public CustomNodeJointLimits JointLimits;
            public bool JointEnableProjection;
            public bool JointEnablePreprocessing;
        }
    }
}
