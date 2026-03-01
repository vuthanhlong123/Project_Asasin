using UnityEngine;
using System.Collections.Generic;

namespace PhoenixRealm.RagdollCreatorPro
{
    public class RagdollPhysicsComponents : MonoBehaviour
    {
        private List<Rigidbody> m_rigidbodies = new List<Rigidbody>();
        private List<Collider> m_colliders = new List<Collider>();
        private List<CharacterJoint> m_joints = new List<CharacterJoint>();
        private bool m_componentsStripped = false;

        public void CacheComponents()
        {
            m_rigidbodies.Clear();
            m_colliders.Clear();
            m_joints.Clear();

            m_rigidbodies.AddRange(GetComponentsInChildren<Rigidbody>(true));
            m_colliders.AddRange(GetComponentsInChildren<Collider>(true));
            m_joints.AddRange(GetComponentsInChildren<CharacterJoint>(true));
        }

        public void StripPhysicsComponents()
        {
            if (m_componentsStripped) return;

            foreach (var rb in m_rigidbodies)
            {
                if (rb == null) continue;

                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.useGravity = false;
                rb.isKinematic = true;
                rb.detectCollisions = false;
            }

            foreach (var col in m_colliders)
            {
                if (col != null)
                {
                    col.enabled = false;
                }
            }

            m_componentsStripped = true;
        }

        public void RestorePhysicsComponents()
        {
            if (!m_componentsStripped) return;

            foreach (var col in m_colliders)
            {
                if (col != null)
                {
                    col.enabled = true;
                }
            }

            foreach (var rb in m_rigidbodies)
            {
                if (rb == null) continue;

                rb.detectCollisions = true;
                rb.useGravity = true;
                rb.isKinematic = false;
            }

            m_componentsStripped = false;
        }

        public bool AreComponentsStripped => m_componentsStripped;
    }
}
