// /Assets/Scripts/Runtime/RagdollSystem/RagdollManager.cs
using UnityEngine;
using System.Collections.Generic;

namespace PhoenixRealm.RagdollCreatorPro
{
    public class RagdollManager : MonoBehaviour
    {
        public static RagdollManager Instance;

        #region Vars + Properties

        [Header("Configuration")]
        [SerializeField] private RagdollLODConfig m_config;

        [Header("Pool Setup")]
        [SerializeField] private List<GameObject> m_ragdollPrefabs = new List<GameObject>();
        [SerializeField] private bool m_prewarmOnAwake = true;

        [Header("Reference Point (usually main camera)")]
        [SerializeField] private Transform m_referencePoint;

        [Header("Debug")]
        [SerializeField] private bool m_showDebugInfo = false;

        private Dictionary<GameObject, RagdollPoolData> m_pools = new Dictionary<GameObject, RagdollPoolData>();
        private List<RagdollInstance> m_activeRagdolls = new List<RagdollInstance>();
        private HashSet<RagdollInstance> m_fullPhysicsRagdolls = new HashSet<RagdollInstance>();

        private float m_lastLODUpdateTime;
        private Camera m_mainCamera;

        public int ActiveRagdollCount => m_activeRagdolls.Count;
        public int FullPhysicsRagdollCount => m_fullPhysicsRagdolls.Count;

        public System.Action<RagdollInstance> OnRagdollSpawned;
        public System.Action<RagdollInstance> OnRagdollDespawned;

        #endregion

        #region Unity Functions

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            if (m_config == null)
            {
                m_config = ScriptableObject.CreateInstance<RagdollLODConfig>();
                Debug.LogWarning("[Ragdoll Manager] No LOD config assigned, using default settings");
            }

            m_mainCamera = Camera.main;

            if (m_prewarmOnAwake)
            {
                PrewarmPools();
            }
        }

        private void Update()
        {
            UpdateReferencePoint();
            UpdateLODSystem();
            UpdateRagdollLifetimes();
            CleanupExpiredRagdolls();
        }

        private void OnGUI()
        {
            if (!m_showDebugInfo) return;

            int yOffset = 10;
            GUI.Box(new Rect(10, yOffset, 350, 180), "Ragdoll Manager Stats");
            yOffset += 25;

            GUI.Label(new Rect(20, yOffset, 330, 20), $"Active Ragdolls: {m_activeRagdolls.Count}");
            yOffset += 20;
            GUI.Label(new Rect(20, yOffset, 330, 20), $"Full Physics: {m_fullPhysicsRagdolls.Count}/{m_config.MaxActivePhysicsRagdolls}");
            yOffset += 20;
            GUI.Label(new Rect(20, yOffset, 330, 20), $"Frozen: {GetRagdollCountByState(RagdollLODState.FrozenPhysics)}");
            yOffset += 20;
            GUI.Label(new Rect(20, yOffset, 330, 20), $"Disabled: {GetRagdollCountByState(RagdollLODState.Disabled)}");
            yOffset += 20;

            GUI.Label(new Rect(20, yOffset, 330, 20), "--- Pool Stats ---");
            yOffset += 20;

            foreach (var kvp in m_pools)
            {
                var pool = kvp.Value;
                GUI.Label(new Rect(20, yOffset, 330, 20),
                    $"{kvp.Key.name}: Pooled={pool.Pool.Count} InUse={pool.TotalInUse} Total={pool.TotalCreated}");
                yOffset += 20;
            }
        }

        #endregion

        #region Pool Setup
        public void PrewarmPools()
        {
            foreach (var prefab in m_ragdollPrefabs)
            {
                if (prefab == null) continue;
                PrewarmPool(prefab, m_config.PrewarmPoolSize);
            }

            Debug.Log($"[Ragdoll Manager] Prewarmed {m_ragdollPrefabs.Count} ragdoll pools");
        }

        public void PrewarmPool(GameObject prefab, int count)
        {
            if (!m_pools.ContainsKey(prefab))
            {
                m_pools[prefab] = new RagdollPoolData(prefab);
            }

            var poolData = m_pools[prefab];

            for (int i = 0; i < count; i++)
            {
                var instance = CreateNewRagdoll(prefab);
                instance.gameObject.SetActive(false);
                poolData.Pool.Enqueue(instance);
            }

            Debug.Log($"[Ragdoll Manager] Prewarmed pool for {prefab.name}: {count} ragdolls");
        }

        public void RegisterPrefab(GameObject prefab)
        {
            if (prefab == null) return;

            if (!m_ragdollPrefabs.Contains(prefab))
            {
                m_ragdollPrefabs.Add(prefab);

                if (m_prewarmOnAwake && Application.isPlaying)
                {
                    PrewarmPool(prefab, m_config.PrewarmPoolSize);
                }
            }
        }

        #endregion

        #region Public API

        public RagdollInstance SpawnRagdoll(RagdollSpawnData spawnData)
        {
            if (spawnData == null || spawnData.RagdollPrefab == null)
            {
                Debug.LogError("[Ragdoll Manager] Invalid spawn data");
                return null;
            }

            RegisterPrefab(spawnData.RagdollPrefab);

            var instance = GetRagdollFromPool(spawnData.RagdollPrefab);

            instance.transform.SetParent(null);
            instance.transform.position = spawnData.BoneSnapshots[0].Position;
            instance.transform.rotation = spawnData.BoneSnapshots[0].Rotation;

            instance.gameObject.SetActive(true);
            instance.Initialize(spawnData, m_config);

            m_activeRagdolls.Add(instance);
            m_fullPhysicsRagdolls.Add(instance);

            EnforcePhysicsBudget();

            OnRagdollSpawned?.Invoke(instance);

            return instance;
        }

        public void DespawnRagdoll(RagdollInstance ragdoll)
        {
            if (ragdoll == null) return;

            m_activeRagdolls.Remove(ragdoll);
            m_fullPhysicsRagdolls.Remove(ragdoll);

            ragdoll.ResetForPooling();
            ragdoll.gameObject.SetActive(false);

            ReturnToPool(ragdoll);

            OnRagdollDespawned?.Invoke(ragdoll);
        }

        public void SetConfig(RagdollLODConfig config)
        {
            m_config = config;
        }

        public void SetReferencePoint(Transform reference)
        {
            m_referencePoint = reference;
        }

        #endregion

        #region Pooling

        private RagdollInstance GetRagdollFromPool(GameObject prefab)
        {
            if (!m_pools.ContainsKey(prefab))
            {
                m_pools[prefab] = new RagdollPoolData(prefab);
            }

            var poolData = m_pools[prefab];

            RagdollInstance instance = null;

            if (poolData.Pool.Count > 0)
            {
                instance = poolData.Pool.Dequeue();
            }
            else
            {
                instance = CreateNewRagdoll(prefab);
            }

            return instance;
        }

        private RagdollInstance CreateNewRagdoll(GameObject prefab)
        {
            var poolData = m_pools[prefab];

            var go = Instantiate(prefab);
            var instance = go.GetComponent<RagdollInstance>();

            if (instance == null)
            {
                instance = go.AddComponent<RagdollInstance>();
            }

            poolData.TotalCreated++;

            return instance;
        }

        private void ReturnToPool(RagdollInstance ragdoll)
        {
            GameObject prefab = FindPrefabForInstance(ragdoll);

            if (prefab == null || !m_pools.ContainsKey(prefab))
            {
                Destroy(ragdoll.gameObject);
                return;
            }

            var poolData = m_pools[prefab];

            if (poolData.TotalPooled >= m_config.MaxPoolSize)
            {
                Destroy(ragdoll.gameObject);
                poolData.TotalCreated--;
                return;
            }

            poolData.Pool.Enqueue(ragdoll);
        }

        private GameObject FindPrefabForInstance(RagdollInstance instance)
        {
            foreach (var kvp in m_pools)
            {
                if (kvp.Value.Pool.Contains(instance))
                    return kvp.Key;
            }

            foreach (var prefab in m_ragdollPrefabs)
            {
                if (instance.name.StartsWith(prefab.name))
                    return prefab;
            }

            return null;
        }

        #endregion

        #region LOD System

        private void UpdateReferencePoint()
        {
            if (m_referencePoint == null && m_mainCamera != null)
            {
                m_referencePoint = m_mainCamera.transform;
            }
        }

        private void UpdateLODSystem()
        {
            if (Time.time - m_lastLODUpdateTime < m_config.LODUpdateInterval)
                return;

            m_lastLODUpdateTime = Time.time;

            if (m_referencePoint == null) return;

            Vector3 refPos = m_referencePoint.position;
            Plane[] frustumPlanes = null;

            if (m_config.EnableFrustumCulling && m_mainCamera != null)
            {
                frustumPlanes = GeometryUtility.CalculateFrustumPlanes(m_mainCamera);
            }

            foreach (var ragdoll in m_activeRagdolls)
            {
                if (ragdoll == null || ragdoll.RootBone == null) continue;

                float distance = Vector3.Distance(refPos, ragdoll.RootBone.position);
                bool inFrustum = true;

                if (frustumPlanes != null && ragdoll.RootBone != null)
                {
                    var bounds = new Bounds(ragdoll.RootBone.position, Vector3.one * 2f);
                    inFrustum = GeometryUtility.TestPlanesAABB(frustumPlanes, bounds);
                }

                RagdollLODState previousState = ragdoll.CurrentLODState;
                RagdollLODState targetState = DetermineLODState(distance, inFrustum);

                if (previousState != targetState)
                {
                    ragdoll.SetLODState(targetState, m_config);
                    UpdatePhysicsTracking(ragdoll, targetState);
                }
            }

            EnforcePhysicsBudget();
        }

        private RagdollLODState DetermineLODState(float distance, bool inFrustum)
        {
            if (!inFrustum && m_config.EnableFrustumCulling)
            {
                if (distance > m_config.FrozenPhysicsDistance)
                    return RagdollLODState.Disabled;
                else
                    return RagdollLODState.FrozenPhysics;
            }

            if (distance <= m_config.FullPhysicsDistance)
                return RagdollLODState.FullPhysics;
            else if (distance <= m_config.FrozenPhysicsDistance)
                return RagdollLODState.FrozenPhysics;
            else
                return RagdollLODState.Disabled;
        }

        private void UpdatePhysicsTracking(RagdollInstance ragdoll, RagdollLODState newState)
        {
            if (newState == RagdollLODState.FullPhysics)
            {
                if (!m_fullPhysicsRagdolls.Contains(ragdoll))
                    m_fullPhysicsRagdolls.Add(ragdoll);
            }
            else
            {
                m_fullPhysicsRagdolls.Remove(ragdoll);
            }
        }

        private void EnforcePhysicsBudget()
        {
            while (m_fullPhysicsRagdolls.Count > m_config.MaxActivePhysicsRagdolls)
            {
                var oldest = FindOldestFullPhysicsRagdoll();
                if (oldest != null)
                {
                    oldest.SetLODState(RagdollLODState.FrozenPhysics, m_config);
                    m_fullPhysicsRagdolls.Remove(oldest);
                }
                else
                {
                    break;
                }
            }
        }

        private RagdollInstance FindOldestFullPhysicsRagdoll()
        {
            RagdollInstance oldest = null;
            float maxAge = 0f;

            foreach (var ragdoll in m_fullPhysicsRagdolls)
            {
                if (ragdoll == null) continue;

                if (ragdoll.TimeAlive > maxAge)
                {
                    maxAge = ragdoll.TimeAlive;
                    oldest = ragdoll;
                }
            }

            return oldest;
        }

        #endregion

        #region Lifetime Management

        private void UpdateRagdollLifetimes()
        {
            foreach (var ragdoll in m_activeRagdolls)
            {
                if (ragdoll == null) continue;

                if (ragdoll.IsExpired && !ragdoll.IsFadingOut)
                {
                    ragdoll.StartFadeOut();
                }

                if (ragdoll.IsFadingOut)
                {
                    ragdoll.UpdateFadeOut();
                }
            }
        }

        private void CleanupExpiredRagdolls()
        {
            for (int i = m_activeRagdolls.Count - 1; i >= 0; i--)
            {
                var ragdoll = m_activeRagdolls[i];

                if (ragdoll == null || ragdoll.ShouldDespawn(m_config))
                {
                    DespawnRagdoll(ragdoll);
                }
            }
        }

        #endregion

        #region Getters

        private int GetRagdollCountByState(RagdollLODState state)
        {
            int count = 0;
            foreach (var ragdoll in m_activeRagdolls)
            {
                if (ragdoll != null && ragdoll.CurrentLODState == state)
                    count++;
            }
            return count;
        }

        public Dictionary<GameObject, RagdollPoolData> GetPoolStats()
        {
            return new Dictionary<GameObject, RagdollPoolData>(m_pools);
        }

        #endregion
    }
}
