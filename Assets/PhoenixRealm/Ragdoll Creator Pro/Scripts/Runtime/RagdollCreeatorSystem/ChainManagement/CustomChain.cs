using System.Collections.Generic;
using UnityEngine;

namespace PhoenixRealm.RagdollCreatorPro
{
    [System.Serializable]
    public class CustomChain
    {
        #region Vars + Properties

        [SerializeField] private float m_totalMass = 50f;
        [SerializeField] private CustomNode m_parentNode = null;

        [SerializeField] private string m_chainName = "New Chain";
        [SerializeField] private List<CustomNode> m_nodes = new List<CustomNode>();
        [SerializeField] private int m_childrenDepth = 3;
        [SerializeField] private ColliderType m_defaultColliderType = ColliderType.Capsule;
        [SerializeField] private float m_defaultMass = 1f;
        [SerializeField] private bool m_jointStabilityDefault = true;

        public float TotalMass
        {
            get => m_totalMass;
            set => m_totalMass = Mathf.Max(0.1f, value);
        }

        public string ChainName
        {
            get => m_chainName;
            set => m_chainName = value;
        }

        public List<CustomNode> Nodes
        {
            get => m_nodes;
            set => m_nodes = value;
        }

        public int ChildrenDepth
        {
            get => m_childrenDepth;
            set => m_childrenDepth = Mathf.Max(1, value);
        }

        public ColliderType DefaultColliderType
        {
            get => m_defaultColliderType;
            set => m_defaultColliderType = value;
        }

        public float DefaultMass
        {
            get => m_defaultMass;
            set => m_defaultMass = Mathf.Max(0.1f, value);
        }

        public bool JointStabilityDefault
        {
            get => m_jointStabilityDefault;
            set => m_jointStabilityDefault = value;
        }

        /// <summary>Optional parent node for connecting the first node of this chain to another chain</summary>
        public CustomNode ParentNode
        {
            get => m_parentNode;
            set => m_parentNode = value;
        }

        #endregion

        #region Custom Functions

        public CustomChain()
        {
            m_nodes = new List<CustomNode>();
        }

        public CustomChain(string name) : this()
        {
            m_chainName = name;
        }
        public CustomChain(string name, float totalMass) : this()
        {
            m_chainName = name;
            m_totalMass = totalMass;
        }

        public void AddNode(Transform transform)
        {
            var node = new CustomNode(transform)
            {
                ColliderType = m_defaultColliderType,
                MassOverride = m_defaultMass,
                JointStability = m_jointStabilityDefault,
                JointLimits = CustomNodeJointLimits.Default()
            };

            m_nodes.Add(node);
        }

        public void RemoveNode(int index)
        {
            if (index >= 0 && index < m_nodes.Count)
            {
                m_nodes.RemoveAt(index);
            }
        }

        public void AutoFillChildren()
        {
            if (m_nodes.Count == 0 || m_nodes[0].Transform == null)
                return;

            var rootTransform = m_nodes[0].Transform;
            int count = Mathf.Clamp(m_childrenDepth, 0, rootTransform.childCount);

            for (int i = 0; i < count; i++)
            {
                var child = rootTransform.GetChild(i);
                if (child == null)
                    continue;

                var existingNode = m_nodes.Find(n => n.Transform == child);
                if (existingNode == null)
                {
                    AddNode(child);
                }
            }
        }

        public void DistributeMassAcrossNodes()
        {
            if (m_nodes.Count == 0) return;

            float totalWeight = 0f;
            float currentWeight = 1f;

            for (int i = 0; i < m_nodes.Count; i++)
            {
                totalWeight += currentWeight;
                currentWeight *= 0.5f;
            }

            float currentMassWeight = 1f;
            for (int i = 0; i < m_nodes.Count; i++)
            {
                float massRatio = currentMassWeight / totalWeight;
                m_nodes[i].MassOverride = m_totalMass * massRatio;
                currentMassWeight *= 0.5f;
            }
        }

        public bool IsValid(out List<string> errorMessages)
        {
            errorMessages = new List<string>();

            if (string.IsNullOrEmpty(m_chainName))
            {
                errorMessages.Add("Chain has no name");
            }

            if (m_nodes.Count == 0)
            {
                errorMessages.Add($"Chain '{m_chainName}' has no nodes");
                return false;
            }

            var transformSet = new HashSet<Transform>();
            for (int i = 0; i < m_nodes.Count; i++)
            {
                var node = m_nodes[i];

                if (!node.IsValid(out string nodeError))
                {
                    errorMessages.Add(nodeError);
                }

                if (node.Transform != null)
                {
                    if (transformSet.Contains(node.Transform))
                    {
                        errorMessages.Add($"Duplicate transform '{node.Transform.name}' in chain '{m_chainName}'");
                    }
                    else
                    {
                        transformSet.Add(node.Transform);
                    }
                }
            }

            return errorMessages.Count == 0;
        }

        public CustomChain Clone()
        {
            var clone = new CustomChain
            {
                m_chainName = m_chainName + " Copy",
                m_totalMass = m_totalMass,
                m_childrenDepth = m_childrenDepth,
                m_defaultColliderType = m_defaultColliderType,
                m_defaultMass = m_defaultMass,
                m_jointStabilityDefault = m_jointStabilityDefault,
                m_parentNode = null
            };

            clone.m_nodes = new List<CustomNode>();
            foreach (var node in m_nodes)
            {
                clone.m_nodes.Add(node.Clone());
            }

            return clone;
        }

        #endregion

        #region Getters

        public int NodeCount => m_nodes.Count;
        public CustomNode RootNode => m_nodes.Count > 0 ? m_nodes[0] : null;

        #endregion
    }
}
