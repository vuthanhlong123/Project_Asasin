using UnityEngine;

namespace PhoenixRealm.RagdollCreatorPro
{
    [System.Serializable]
    public struct RagdollBoneSnapshot
    {
        public Transform SourceBone;
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Velocity;
        public Vector3 AngularVelocity;

        public RagdollBoneSnapshot(Transform bone, Rigidbody rb = null)
        {
            SourceBone = bone;
            Position = bone.position;
            Rotation = bone.rotation;

            if (rb != null)
            {
                Velocity = rb.linearVelocity;
                AngularVelocity = rb.angularVelocity;
            }
            else
            {
                Velocity = Vector3.zero;
                AngularVelocity = Vector3.zero;
            }
        }
    }

    public class RagdollSpawnData
    {
        public GameObject RagdollPrefab;
        public RagdollBoneSnapshot[] BoneSnapshots;
        public Vector3 HitPoint;
        public Vector3 HitForce;
        public Vector3 HitNormal;
        public Transform HitBone;
        public float LifeTime = 10f;

        public RagdollSpawnData(GameObject prefab)
        {
            RagdollPrefab = prefab;
        }

        public static RagdollSpawnData CaptureFromAnimator(Animator animator, GameObject ragdollPrefab)
        {
            var data = new RagdollSpawnData(ragdollPrefab);

            if (animator == null) return data;

            var bones = animator.GetComponentsInChildren<Transform>();
            data.BoneSnapshots = new RagdollBoneSnapshot[bones.Length];

            for (int i = 0; i < bones.Length; i++)
            {
                var rb = bones[i].GetComponent<Rigidbody>();
                data.BoneSnapshots[i] = new RagdollBoneSnapshot(bones[i], rb);
            }

            return data;
        }
    }
}
