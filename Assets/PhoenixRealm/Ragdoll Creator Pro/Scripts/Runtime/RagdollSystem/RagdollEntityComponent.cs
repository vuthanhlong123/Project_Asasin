using UnityEngine;

namespace PhoenixRealm.RagdollCreatorPro
{
    public class RagdollEntityComponent : MonoBehaviour, IRagdollEntity
    {
        #region Vars + Properties

        [Header("Ragdoll Settings")]
        [SerializeField] private GameObject m_ragdollPrefab;
        [SerializeField] private Animator m_animator;
        [SerializeField] private Transform m_rootBone;

        [Header("Spawn Settings")]
        [SerializeField] private float m_ragdollLifetime = 10f;

        [Header("Projectile Transfer")]
        [SerializeField] private bool m_transferProjectilesToRagdoll = true;

        private Vector3 m_lastHitPoint;
        private Vector3 m_lastHitForce;
        private Vector3 m_lastHitNormal;
        private Transform m_lastHitBone;

        public GameObject RagdollPrefab => m_ragdollPrefab;

        #endregion

        #region Unity Functions

        private void Awake()
        {
            if (m_animator == null)
                m_animator = GetComponent<Animator>();

            if (m_rootBone == null && m_animator != null)
                m_rootBone = m_animator.GetBoneTransform(HumanBodyBones.Hips);
        }

        #endregion

        #region IRagdollEntity Implementation

        public Animator GetAnimator() => m_animator;

        public Transform GetRootBone() => m_rootBone;

        public void OnRagdollSpawned(RagdollInstance ragdoll)
        {
            if (m_transferProjectilesToRagdoll)
            {
                TransferProjectilesToRagdoll(ragdoll);
            }
        }

        #endregion

        #region Public API

        public void RegisterHit(Vector3 hitPoint, Vector3 hitForce, Vector3 hitNormal, Transform hitBone = null)
        {
            m_lastHitPoint = hitPoint;
            m_lastHitForce = hitForce;
            m_lastHitNormal = hitNormal;
            m_lastHitBone = hitBone;
        }

        public RagdollInstance SpawnRagdoll()
        {
            if (m_ragdollPrefab == null)
            {
                Debug.LogError($"[Ragdoll Entity] No ragdoll prefab assigned for {gameObject.name}");
                return null;
            }

            var spawnData = RagdollSpawnData.CaptureFromAnimator(m_animator, m_ragdollPrefab);
            spawnData.LifeTime = m_ragdollLifetime;
            spawnData.HitPoint = m_lastHitPoint;
            spawnData.HitForce = m_lastHitForce;
            spawnData.HitNormal = m_lastHitNormal;
            spawnData.HitBone = m_lastHitBone;

            var ragdoll = RagdollManager.Instance.SpawnRagdoll(spawnData);
            OnRagdollSpawned(ragdoll);

            return ragdoll;
        }

        public void Die(Vector3 hitPoint, Vector3 hitForce, Vector3 hitNormal, Transform hitBone = null)
        {
            RegisterHit(hitPoint, hitForce, hitNormal, hitBone);
            SpawnRagdoll();

            gameObject.SetActive(false);
        }

        #endregion

        #region Projectile Transfer

        private void TransferProjectilesToRagdoll(RagdollInstance ragdoll)
        {
            if (ragdoll == null)
            {
                Debug.LogWarning("[Ragdoll Entity] Cannot transfer projectiles: ragdoll instance is null");
                return;
            }

            RagdollProjectileTransfer transferComponent = ragdoll.GetComponent<RagdollProjectileTransfer>();

            if (transferComponent == null)
            {
                transferComponent = ragdoll.gameObject.AddComponent<RagdollProjectileTransfer>();
            }

            Transform originalRoot = m_rootBone != null ? m_rootBone : transform;
            Transform ragdollRoot = ragdoll.RootBone;

            transferComponent.TransferProjectilesFromOriginalToRagdoll(originalRoot, ragdollRoot);
        }

        #endregion
    }
}
