using UnityEngine;

namespace PhoenixRealm.RagdollCreatorPro
{
    public interface IRagdollEntity
    {
        GameObject RagdollPrefab { get; }

        Animator GetAnimator();

        Transform GetRootBone();

        void OnRagdollSpawned(RagdollInstance ragdoll);
    }
}
