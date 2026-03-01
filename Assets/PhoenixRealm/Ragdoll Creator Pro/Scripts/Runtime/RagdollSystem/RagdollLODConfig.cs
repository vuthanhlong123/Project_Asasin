using UnityEngine;

namespace PhoenixRealm.RagdollCreatorPro
{
    [CreateAssetMenu(fileName = "RagdollLODConfig", menuName = "Ragdoll Maker/Runtime/LOD Configuration")]
    public class RagdollLODConfig : ScriptableObject
    {
        [Header("Pool Settings")]
        [Tooltip("Number of ragdolls to pre-spawn per prefab")]
        public int PrewarmPoolSize = 50;

        [Tooltip("Maximum pool size before destroying excess ragdolls")]
        public int MaxPoolSize = 100;

        [Header("LOD Distance Thresholds")]
        [Tooltip("Distance for full physics simulation")]
        public float FullPhysicsDistance = 30f;

        [Tooltip("Distance where physics freeze but visual remains")]
        public float FrozenPhysicsDistance = 60f;

        [Tooltip("Distance where ragdoll is completely disabled")]
        public float DisableDistance = 100f;

        [Header("Performance Settings")]
        [Tooltip("Maximum number of ragdolls with active physics")]
        public int MaxActivePhysicsRagdolls = 10;

        [Tooltip("Update frequency for LOD checks (seconds)")]
        public float LODUpdateInterval = 0.5f;

        [Header("Lifetime Settings")]
        [Tooltip("Default ragdoll lifetime before despawn")]
        public float DefaultLifetime = 10f;

        [Tooltip("Fade out duration before despawn")]
        public float FadeOutDuration = 2f;

        [Header("Physics Settings")]
        [Tooltip("Reduce physics iterations when frozen")]
        public bool ReduceIterationsWhenFrozen = true;

        [Tooltip("Solver iterations for frozen ragdolls")]
        public int FrozenSolverIterations = 2;

        [Tooltip("Solver iterations for active ragdolls")]
        public int ActiveSolverIterations = 6;

        [Header("Culling")]
        [Tooltip("Despawn ragdolls that fall below this Y position")]
        public float DespawnBelowY = -50f;

        [Tooltip("Enable frustum culling optimization")]
        public bool EnableFrustumCulling = true;

        [Header("Frozen Optimization")]
        [Tooltip("Strip all physics components from frozen ragdolls")]
        public bool StripPhysicsWhenFrozen = true;
    }
}
