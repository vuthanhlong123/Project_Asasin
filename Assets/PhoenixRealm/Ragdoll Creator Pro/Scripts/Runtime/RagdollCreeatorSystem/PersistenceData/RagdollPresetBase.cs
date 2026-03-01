using System.Collections.Generic;
using UnityEngine;

namespace PhoenixRealm.RagdollCreatorPro
{
    public abstract class RagdollPresetBase : ScriptableObject
    {
        #region Vars + Properties

        [SerializeField] private string m_presetName = "Ragdoll Preset";
        [SerializeField] private string m_description = "";
        [SerializeField] private List<PresetChainData> m_chainTemplates = new List<PresetChainData>();

        public string PresetName
        {
            get => m_presetName;
            set => m_presetName = value;
        }

        public string Description
        {
            get => m_description;
            set => m_description = value;
        }

        public List<PresetChainData> ChainTemplates
        {
            get => m_chainTemplates;
            set => m_chainTemplates = value;
        }

        #endregion

        #region Custom Functions

        public List<CustomChain> CreateChainsFromPreset()
        {
            var chains = new List<CustomChain>();

            foreach (var template in m_chainTemplates)
            {
                var chain = CreateChainFromTemplate(template);
                chains.Add(chain);
            }

            return chains;
        }

        public void CaptureFromChains(List<CustomChain> sourceChains)
        {
            m_chainTemplates.Clear();

            foreach (var sourceChain in sourceChains)
            {
                var template = CreateTemplateFromChain(sourceChain);
                m_chainTemplates.Add(template);
            }
        }

        #endregion

        #region Getters

        private CustomChain CreateChainFromTemplate(PresetChainData template)
        {
            var chain = new CustomChain(template.ChainName, template.TotalMass);

            foreach (var nodeTemplate in template.NodeTemplates)
            {
                var node = new CustomNode(null)
                {
                    NodeName = nodeTemplate.NodeName,
                    ColliderType = nodeTemplate.ColliderType,
                    ColliderSize = nodeTemplate.ColliderSize,
                    ColliderRadius = nodeTemplate.ColliderRadius,
                    ColliderHeight = nodeTemplate.ColliderHeight,
                    CapsuleDirection = nodeTemplate.CapsuleDirection,
                    LocalOffset = nodeTemplate.LocalOffset,
                    MassOverride = nodeTemplate.MassOverride,
                    JointStability = nodeTemplate.JointStability,
                    JointAnchorLocal = nodeTemplate.JointAnchorLocal,
                    JointConnectedAnchorLocal = nodeTemplate.JointConnectedAnchorLocal,
                    JointAxisLocal = nodeTemplate.JointAxisLocal,
                    JointLimits = nodeTemplate.JointLimits.Clone(),
                    JointEnableProjection = nodeTemplate.JointEnableProjection,
                    JointEnablePreprocessing = nodeTemplate.JointEnablePreprocessing
                };

                chain.Nodes.Add(node);
            }

            return chain;
        }

        private PresetChainData CreateTemplateFromChain(CustomChain sourceChain)
        {
            var template = new PresetChainData
            {
                ChainName = sourceChain.ChainName,
                TotalMass = sourceChain.TotalMass,
                NodeTemplates = new List<PresetNodeData>()
            };

            foreach (var node in sourceChain.Nodes)
            {
                var nodeTemplate = new PresetNodeData
                {
                    NodeName = node.NodeName,
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
                    JointLimits = node.JointLimits.Clone(),
                    JointEnableProjection = node.JointEnableProjection,
                    JointEnablePreprocessing = node.JointEnablePreprocessing
                };

                template.NodeTemplates.Add(nodeTemplate);
            }

            return template;
        }

        #endregion

        [System.Serializable]
        public class PresetChainData
        {
            public string ChainName;
            public float TotalMass;
            public List<PresetNodeData> NodeTemplates;
        }

        [System.Serializable]
        public class PresetNodeData
        {
            public string NodeName;
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
