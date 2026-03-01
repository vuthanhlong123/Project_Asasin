using UnityEngine;
using System.Collections.Generic;

namespace PhoenixRealm.RagdollCreatorPro
{
    public class RagdollProjectileTransfer : MonoBehaviour
    {

        private List<PhoenixRealm.EntitySystem.ProjectileRagdollTransferable> m_attachedProjectiles = new List<PhoenixRealm.EntitySystem.ProjectileRagdollTransferable>();

        public void TransferProjectilesFromOriginalToRagdoll(Transform originalRootBone, Transform ragdollRootBone)
        {
            if (originalRootBone == null || ragdollRootBone == null)
            {
                return;
            }

            Dictionary<string, Transform> ragdollBoneMap = BuildBoneMap(ragdollRootBone);

            List<PhoenixRealm.EntitySystem.ProjectileRagdollTransferable> transferableProjectiles =
                FindAllTransferableProjectiles(originalRootBone);

            int transferredCount = 0;

            foreach (var projectile in transferableProjectiles)
            {
                if (projectile == null || projectile.CurrentParentBone == null)
                    continue;

                string boneName = projectile.CurrentParentBone.name;

                if (ragdollBoneMap.TryGetValue(boneName, out Transform ragdollBone))
                {
                    projectile.TransferToNewBone(ragdollBone);
                    m_attachedProjectiles.Add(projectile);
                    transferredCount++;
                }
                else if (projectile.TransferToClosestBone)
                {
                    Transform closestBone = FindClosestBone(projectile.transform.position, ragdollBoneMap);
                    if (closestBone != null)
                    {
                        projectile.TransferToNewBone(closestBone);
                        m_attachedProjectiles.Add(projectile);
                        transferredCount++;
                    }
                }
            }
        }

        public void ClearAllAttachedProjectiles()
        {
            foreach (var projectile in m_attachedProjectiles)
            {
                if (projectile != null && projectile.gameObject != null)
                {
                    projectile.gameObject.SetActive(false);
                }
            }

            m_attachedProjectiles.Clear();
        }

        private Dictionary<string, Transform> BuildBoneMap(Transform root)
        {
            Dictionary<string, Transform> boneMap = new Dictionary<string, Transform>();

            Transform[] allBones = root.GetComponentsInChildren<Transform>();
            foreach (Transform bone in allBones)
            {
                if (!boneMap.ContainsKey(bone.name))
                {
                    boneMap.Add(bone.name, bone);
                }
            }

            return boneMap;
        }

        private List<PhoenixRealm.EntitySystem.ProjectileRagdollTransferable> FindAllTransferableProjectiles(Transform root)
        {
            List<PhoenixRealm.EntitySystem.ProjectileRagdollTransferable> projectiles =
                new List<PhoenixRealm.EntitySystem.ProjectileRagdollTransferable>();

            Transform[] allTransforms = root.GetComponentsInChildren<Transform>();

            foreach (Transform t in allTransforms)
            {
                PhoenixRealm.EntitySystem.ProjectileRagdollTransferable[] componentsOnTransform =
                    t.GetComponents<PhoenixRealm.EntitySystem.ProjectileRagdollTransferable>();

                foreach (var projectile in componentsOnTransform)
                {
                    if (projectile != null)
                    {
                        projectile.UpdateBoneAttachment();
                        projectiles.Add(projectile);
                    }
                }
            }

            return projectiles;
        }

        private Transform FindClosestBone(Vector3 position, Dictionary<string, Transform> boneMap)
        {
            Transform closestBone = null;
            float closestDistance = float.MaxValue;

            foreach (var bone in boneMap.Values)
            {
                float distance = Vector3.Distance(position, bone.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestBone = bone;
                }
            }

            return closestBone;
        }

        private void OnDestroy()
        {
            ClearAllAttachedProjectiles();
        }
    }
}
