using UnityEngine;

namespace PhoenixRealm.EntitySystem
{
    public class ProjectileRagdollTransferable : MonoBehaviour
    {
        [Header("Transfer Settings")]
        [Tooltip("If true, the projectile will maintain its local transform when transferred")]
        [SerializeField] private bool m_maintainLocalTransform = true;

        [Tooltip("If true, the projectile will be transferred even if the bone is not found on the ragdoll")]
        [SerializeField] private bool m_transferToClosestBone = false;

        public bool MaintainLocalTransform => m_maintainLocalTransform;
        public bool TransferToClosestBone => m_transferToClosestBone;

        public Transform CurrentParentBone { get; private set; }
        public Vector3 LocalPositionOnBone { get; private set; }
        public Quaternion LocalRotationOnBone { get; private set; }

        private void LateUpdate()
        {
            if (transform.parent != null && transform.parent != CurrentParentBone)
            {
                UpdateBoneAttachment();
            }
        }

        public void UpdateBoneAttachment()
        {
            CurrentParentBone = transform.parent;
            if (CurrentParentBone != null)
            {
                LocalPositionOnBone = transform.localPosition;
                LocalRotationOnBone = transform.localRotation;
            }
        }

        public void TransferToNewBone(Transform newBone)
        {
            if (newBone == null)
            {
                Debug.LogWarning($"[ProjectileRagdollTransferable] Cannot transfer {gameObject.name} to null bone");
                return;
            }

            transform.SetParent(newBone);

            if (m_maintainLocalTransform)
            {
                transform.localPosition = LocalPositionOnBone;
                transform.localRotation = LocalRotationOnBone;
            }

            CurrentParentBone = newBone;
        }
    }
}
