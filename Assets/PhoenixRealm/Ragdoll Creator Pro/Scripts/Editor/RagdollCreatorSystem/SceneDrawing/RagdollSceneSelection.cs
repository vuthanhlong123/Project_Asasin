#if UNITY_EDITOR
using PhoenixRealm.RagdollCreatorPro;
using System;
using UnityEngine;

namespace PhoenixRealm.RagdollCreatorPro
{
    /// <summary>
    /// Global scene selection for ragdoll nodes, independent of any MonoBehaviour.
    /// </summary>
    internal static class RagdollSceneSelection
    {
        #region Vars + Properties
        private static int s_chain = -1;
        private static int s_node = -1;
        public static event Action SelectionChanged;
        #endregion

        #region Custom Functions
        public static bool TryGetSelected(out int chainIdx, out int nodeIdx)
        {
            chainIdx = s_chain; nodeIdx = s_node;
            return s_chain >= 0 && s_node >= 0;
        }

        public static bool IsSelected(int chainIdx, int nodeIdx)
        {
            return s_chain == chainIdx && s_node == nodeIdx;
        }

        public static void SetSelection(int chainIdx, int nodeIdx)
        {
            s_chain = chainIdx;
            s_node = nodeIdx;
            SelectionChanged?.Invoke();
        }

        public static void Clear()
        {
            if (s_chain < 0 && s_node < 0) return;
            s_chain = -1; s_node = -1;
            SelectionChanged?.Invoke();
        }
        #endregion
    }
}
#endif
