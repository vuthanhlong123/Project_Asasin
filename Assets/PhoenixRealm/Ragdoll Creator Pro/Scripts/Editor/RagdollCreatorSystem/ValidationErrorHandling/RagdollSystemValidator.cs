#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace PhoenixRealm.RagdollCreatorPro.Editor
{
    public static class RagdollSystemValidator
    {
        #region Custom Functions

        /// <summary>Validates all chains and returns consolidated error messages</summary>
        public static ValidationResult ValidateChains(List<CustomChain> chains)
        {
            var result = new ValidationResult();

            if (chains == null || chains.Count == 0)
            {
                result.AddError("No chains defined");
                return result;
            }

            var chainNames = new HashSet<string>();

            foreach (var chain in chains)
            {
                // Check for duplicate chain names
                if (chainNames.Contains(chain.ChainName))
                {
                    result.AddError($"Duplicate chain name: '{chain.ChainName}'");
                }
                else
                {
                    chainNames.Add(chain.ChainName);
                }

                // Validate individual chain
                if (chain.IsValid(out List<string> chainErrors))
                {
                    result.AddInfo($"Chain '{chain.ChainName}' is valid ({chain.NodeCount} nodes)");
                }
                else
                {
                    foreach (var error in chainErrors)
                    {
                        result.AddError($"Chain '{chain.ChainName}': {error}");
                    }
                }

                // Check chain continuity
                ValidateChainContinuity(chain, result);
            }

            return result;
        }

        /// <summary>Validates that nodes in a chain form a continuous hierarchy</summary>
        public static void ValidateChainContinuity(CustomChain chain, ValidationResult result)
        {
            for (int i = 1; i < chain.Nodes.Count; i++)
            {
                var currentNode = chain.Nodes[i];
                var previousNode = chain.Nodes[i - 1];

                if (currentNode.Transform == null || previousNode.Transform == null)
                    continue;

                // Check if current node is a child of the previous node
                bool isChild = currentNode.Transform.parent == previousNode.Transform;
                bool isDescendant = IsDescendantOf(currentNode.Transform, previousNode.Transform);

                if (!isChild && !isDescendant)
                {
                    result.AddWarning($"Chain '{chain.ChainName}': Node '{currentNode.NodeName}' is not a descendant of '{previousNode.NodeName}'");
                }
            }
        }

        /// <summary>Checks if a transform is a descendant of another</summary>
        public static bool IsDescendantOf(Transform child, Transform ancestor)
        {
            Transform current = child.parent;
            while (current != null)
            {
                if (current == ancestor)
                    return true;
                current = current.parent;
            }
            return false;
        }

        #endregion
    }

    public class ValidationResult
    {
        #region Vars + Properties

        private readonly List<string> m_errors = new List<string>();
        private readonly List<string> m_warnings = new List<string>();
        private readonly List<string> m_infos = new List<string>();

        public List<string> Errors => m_errors;
        public List<string> Warnings => m_warnings;
        public List<string> Infos => m_infos;
        public bool HasErrors => m_errors.Count > 0;
        public bool HasWarnings => m_warnings.Count > 0;

        #endregion

        #region Custom Functions

        public void AddError(string message) => m_errors.Add(message);
        public void AddWarning(string message) => m_warnings.Add(message);
        public void AddInfo(string message) => m_infos.Add(message);

        public void Clear()
        {
            m_errors.Clear();
            m_warnings.Clear();
            m_infos.Clear();
        }

        #endregion
    }
}
#endif
