#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using PhoenixRealm.RagdollCreatorPro;

namespace PhoenixRealm.RagdollCreatorPro.Editor
{
    public class RagdollCharacterManager
    {
        #region Vars + Properties

        private RagdollMakerContext m_ctx;
        public System.Action OnCharacterChanged;

        #endregion

        #region Constructor

        public RagdollCharacterManager(RagdollMakerContext ctx)
        {
            m_ctx = ctx;
        }

        #endregion

        #region Character Management

        public void SetTargetCharacter(GameObject character)
        {
            if (m_ctx.TargetCharacter != character)
            {
                m_ctx.TargetCharacter = character;
                UpdateTargetMap();
                OnCharacterChanged?.Invoke();
            }
        }

        public void AutoAssignSelectedCharacter()
        {
            var selected = Selection.activeGameObject;

            if (selected != null && IsLikelyCharacter(selected))
            {
                SetTargetCharacter(selected);
            }
            else
            {
                SetTargetCharacter(null);
            }
        }

        public void CreateRagdollMap()
        {
            if (m_ctx.TargetCharacter == null)
            {
                EditorUtility.DisplayDialog("Create RagdollCreatorPro Map Failed",
                    "No character assigned. Please assign a character first.", "OK");
                return;
            }

            var map = CreateAndConfigureRagdollMap();
            UpdateContextWithNewMap(map);
            OnCharacterChanged?.Invoke();
        }

        public void SyncFromCharacter()
        {
            if (m_ctx?.TargetCharacter == null)
            {
                EditorUtility.DisplayDialog("Load Failed",
                    "No character assigned. Please assign a character first.", "OK");
                return;
            }

            var ragdollMap = m_ctx.TargetCharacter.GetComponent<RagdollMap>();
            if (ragdollMap == null)
            {
                EditorUtility.DisplayDialog("Load Failed",
                    "Character has no RagdollCreatorPro Map component. Create one first.", "OK");
                return;
            }

            ragdollMap.PushToChains(m_ctx.Chains);
            Debug.Log($"[RagdollCreatorPro Maker] Loaded {m_ctx.Chains.Count} chains from character '{m_ctx.TargetCharacter.name}'");
            OnCharacterChanged?.Invoke();
        }

        public void SyncToCharacter()
        {
            if (m_ctx?.TargetCharacter == null)
            {
                EditorUtility.DisplayDialog("Save Failed",
                    "No character assigned. Please assign a character first.", "OK");
                return;
            }

            var ragdollMap = m_ctx.TargetCharacter.GetComponent<RagdollMap>();
            if (ragdollMap == null)
            {
                ragdollMap = Undo.AddComponent<RagdollMap>(m_ctx.TargetCharacter);
            }

            Undo.RecordObject(ragdollMap, "Update RagdollCreatorPro Data");
            ragdollMap.PullFromChains(m_ctx.Chains);
            EditorUtility.SetDirty(ragdollMap);
        }

        public void BakeRagdoll()
        {
            if (!CanBakeRagdoll()) return;
            if (!ValidateBeforeBaking()) return;

            PerformBaking();
        }

        public void HandleSceneSelectionChanged()
        {
            if (RagdollSceneSelection.TryGetSelected(out var c, out var n))
            {
                UpdateContextSelection(c, n);
            }
            else
            {
                ClearContextSelection();
            }
        }

        #endregion

        #region Private Methods

        private bool IsLikelyCharacter(GameObject obj)
        {
            if (obj == null) return false;

            return obj.GetComponent<Animator>() != null ||
                   obj.GetComponent<RagdollMap>() != null ||
                   obj.GetComponentInChildren<Animator>() != null;
        }

        private void UpdateTargetMap()
        {
            m_ctx.TargetMap = m_ctx.TargetCharacter?.GetComponent<RagdollMap>();
        }

        private RagdollMap CreateAndConfigureRagdollMap()
        {
            Undo.RegisterCompleteObjectUndo(m_ctx.TargetCharacter, "Create RagdollCreatorPro Map");
            var map = Undo.AddComponent<RagdollMap>(m_ctx.TargetCharacter);

            if (m_ctx.Chains != null && m_ctx.Chains.Count > 0)
            {
                map.PullFromChains(m_ctx.Chains);
                EditorUtility.SetDirty(map);
            }

            return map;
        }

        private void UpdateContextWithNewMap(RagdollMap map)
        {
            m_ctx.TargetMap = map;
        }

        private bool CanBakeRagdoll()
        {
            if (m_ctx.TargetCharacter == null)
            {
                EditorUtility.DisplayDialog("Bake RagdollCreatorPro Failed",
                    "No character assigned. Please assign a character first.", "OK");
                return false;
            }
            return true;
        }

        private bool ValidateBeforeBaking()
        {
            if (m_ctx.Validation?.HasErrors == true)
            {
                EditorUtility.DisplayDialog("Bake RagdollCreatorPro Failed",
                    "Cannot bake ragdoll due to validation errors. Check the validation section for details.", "OK");
                return false;
            }
            return true;
        }

        private void PerformBaking()
        {
            try
            {
                RagdollMakerBake.BakeRagdoll(m_ctx.Chains);
                EditorUtility.DisplayDialog("Bake RagdollCreatorPro",
                    $"RagdollCreatorPro baking for '{m_ctx.TargetCharacter.name}' completed successfully!", "OK");
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("Bake RagdollCreatorPro Failed",
                    $"Failed to bake ragdoll for '{m_ctx.TargetCharacter.name}'.\n\nError: {e.Message}", "OK");
            }
        }

        private void UpdateContextSelection(int chainIndex, int nodeIndex)
        {
            m_ctx.SelectedChain = chainIndex;
            m_ctx.SelectedNode = nodeIndex;
        }

        private void ClearContextSelection()
        {
            m_ctx.SelectedChain = m_ctx.SelectedNode = -1;
        }

        #endregion
    }
}
#endif
