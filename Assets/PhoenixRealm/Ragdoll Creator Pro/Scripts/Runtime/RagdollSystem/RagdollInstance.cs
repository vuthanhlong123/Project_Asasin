// /Assets/Scripts/Runtime/RagdollSystem/RagdollInstance.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PhoenixRealm.RagdollCreatorPro
{
    public enum RagdollLODState
    {
        FullPhysics,
        FrozenPhysics,
        Disabled
    }

    [System.Serializable]
    public class BoneTransformData
    {
        public string BoneName;
        public Vector3 LocalPosition;
        public Quaternion LocalRotation;
        public Vector3 LocalScale;
    }

    [RequireComponent(typeof(RagdollProjectileTransfer))]
    public class RagdollInstance : MonoBehaviour
    {
        #region Vars + Properties

        private Dictionary<string, Rigidbody> m_boneRigidbodies = new Dictionary<string, Rigidbody>();
        private Dictionary<string, Collider> m_boneColliders = new Dictionary<string, Collider>();
        private Dictionary<string, CharacterJoint> m_boneJoints = new Dictionary<string, CharacterJoint>();
        private List<Renderer> m_renderers = new List<Renderer>();

        private Dictionary<string, BoneTransformData> m_originalBoneTransforms = new Dictionary<string, BoneTransformData>();
        private bool m_hasStoredOriginalPose = false;

        private RagdollLODState m_currentLODState = RagdollLODState.FullPhysics;
        private float m_spawnTime;
        private float m_lifetime = 10f;
        private bool m_isInitialized = false;
        private bool m_isFadingOut = false;
        private float m_fadeStartTime;
        private float m_fadeDuration = 2f;
        private Transform m_rootBone;
        private bool m_physicsEnabled = false;

        private RagdollPhysicsComponents m_physicsComponents;

        public RagdollLODState CurrentLODState => m_currentLODState;
        public float TimeAlive => Time.time - m_spawnTime;
        public bool IsExpired => TimeAlive >= m_lifetime;
        public bool IsFadingOut => m_isFadingOut;
        public Transform RootBone => m_rootBone;
        public int RigidbodyCount => m_boneRigidbodies.Count;
        public bool IsPhysicsStripped => m_physicsComponents != null && m_physicsComponents.AreComponentsStripped;

        #endregion

        #region Unity Functions

        private void Awake()
        {
            CacheBoneComponents();
            StoreOriginalBonePose();

            m_physicsComponents = GetComponent<RagdollPhysicsComponents>();
            if (m_physicsComponents == null)
            {
                m_physicsComponents = gameObject.AddComponent<RagdollPhysicsComponents>();
            }
            m_physicsComponents.CacheComponents();
        }

        #endregion

        #region Initialization

        public void Initialize(RagdollSpawnData spawnData, RagdollLODConfig config)
        {
            m_spawnTime = Time.time;
            m_lifetime = spawnData.LifeTime;
            m_fadeDuration = config.FadeOutDuration;

            CacheBoneComponents();

            if (!m_hasStoredOriginalPose)
            {
                StoreOriginalBonePose();
            }

            DisablePhysicsForSetup();

            ApplyBoneSnapshots(spawnData.BoneSnapshots);

            StartCoroutine(EnablePhysicsAfterSetup(spawnData, config));

            m_isInitialized = true;
        }

        private void CacheBoneComponents()
        {
            m_boneRigidbodies.Clear();
            m_boneColliders.Clear();
            m_boneJoints.Clear();
            m_renderers.Clear();

            var rigidbodies = GetComponentsInChildren<Rigidbody>();
            var colliders = GetComponentsInChildren<Collider>();
            var joints = GetComponentsInChildren<CharacterJoint>();

            m_renderers.AddRange(GetComponentsInChildren<Renderer>());

            if (rigidbodies.Length > 0)
            {
                m_rootBone = rigidbodies[0].transform;
            }

            foreach (var rb in rigidbodies)
            {
                m_boneRigidbodies[rb.transform.name] = rb;
            }

            foreach (var col in colliders)
            {
                m_boneColliders[col.transform.name] = col;
            }

            foreach (var joint in joints)
            {
                m_boneJoints[joint.transform.name] = joint;
            }
        }

        private void StoreOriginalBonePose()
        {
            if (m_hasStoredOriginalPose) return;

            m_originalBoneTransforms.Clear();

            foreach (var kvp in m_boneRigidbodies)
            {
                if (kvp.Value == null) continue;

                Transform boneTransform = kvp.Value.transform;

                var data = new BoneTransformData
                {
                    BoneName = kvp.Key,
                    LocalPosition = boneTransform.localPosition,
                    LocalRotation = boneTransform.localRotation,
                    LocalScale = boneTransform.localScale
                };

                m_originalBoneTransforms[kvp.Key] = data;
            }

            m_hasStoredOriginalPose = true;
        }

        private void RestoreOriginalBonePose()
        {
            if (!m_hasStoredOriginalPose) return;

            foreach (var kvp in m_originalBoneTransforms)
            {
                if (!m_boneRigidbodies.TryGetValue(kvp.Key, out Rigidbody rb)) continue;
                if (rb == null) continue;

                Transform boneTransform = rb.transform;
                var data = kvp.Value;

                boneTransform.localPosition = data.LocalPosition;
                boneTransform.localRotation = data.LocalRotation;
                boneTransform.localScale = data.LocalScale;
            }

            Physics.SyncTransforms();
        }

        private void DisablePhysicsForSetup()
        {
            foreach (var rb in m_boneRigidbodies.Values)
            {
                if (rb == null) continue;

                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
                rb.useGravity = false;
                rb.detectCollisions = false;
            }

            foreach (var col in m_boneColliders.Values)
            {
                if (col != null) col.enabled = false;
            }

            foreach (var joint in m_boneJoints.Values)
            {
                if (joint != null) joint.enableCollision = false;
            }
        }

        private IEnumerator EnablePhysicsAfterSetup(RagdollSpawnData spawnData, RagdollLODConfig config)
        {
            yield return new WaitForFixedUpdate();

            Physics.SyncTransforms();

            foreach (var rb in m_boneRigidbodies.Values)
            {
                if (rb == null) continue;

                rb.useGravity = true;
                rb.detectCollisions = true;
                rb.isKinematic = false;
            }

            foreach (var col in m_boneColliders.Values)
            {
                if (col != null) col.enabled = true;
            }

            foreach (var joint in m_boneJoints.Values)
            {
                if (joint != null) joint.enableCollision = false;
            }

            yield return new WaitForFixedUpdate();

            Physics.SyncTransforms();

            ApplyStoredVelocities(spawnData.BoneSnapshots);

            yield return new WaitForFixedUpdate();

            ApplyHitForce(spawnData.HitPoint, spawnData.HitForce, spawnData.HitNormal, spawnData.HitBone);

            m_physicsEnabled = true;
        }

        #endregion

        #region Bone State Transfer

        private void ApplyBoneSnapshots(RagdollBoneSnapshot[] snapshots)
        {
            if (snapshots == null) return;

            foreach (var snapshot in snapshots)
            {
                if (snapshot.SourceBone == null) continue;

                string boneName = snapshot.SourceBone.name;

                if (m_boneRigidbodies.TryGetValue(boneName, out Rigidbody rb))
                {
                    rb.transform.position = snapshot.Position;
                    rb.transform.rotation = snapshot.Rotation;
                }
            }
        }

        private void ApplyStoredVelocities(RagdollBoneSnapshot[] snapshots)
        {
            if (snapshots == null) return;

            foreach (var snapshot in snapshots)
            {
                if (snapshot.SourceBone == null) continue;

                string boneName = snapshot.SourceBone.name;

                if (m_boneRigidbodies.TryGetValue(boneName, out Rigidbody rb))
                {
                    rb.linearVelocity = snapshot.Velocity;
                    rb.angularVelocity = snapshot.AngularVelocity;
                }
            }
        }

        private void ApplyHitForce(Vector3 hitPoint, Vector3 hitForce, Vector3 hitNormal, Transform hitBone)
        {
            if (hitForce.sqrMagnitude < 0.01f) return;

            Rigidbody targetRb = null;

            if (hitBone != null && m_boneRigidbodies.TryGetValue(hitBone.name, out Rigidbody rb))
            {
                targetRb = rb;
            }
            else if (m_boneRigidbodies.Count > 0)
            {
                targetRb = FindClosestRigidbody(hitPoint);
            }

            if (targetRb != null)
            {
                targetRb.AddForceAtPosition(hitForce, hitPoint, ForceMode.Impulse);

                DistributeForceToNeighbors(targetRb, hitForce * 0.3f);
            }
        }

        private Rigidbody FindClosestRigidbody(Vector3 point)
        {
            Rigidbody closest = null;
            float closestDist = float.MaxValue;

            foreach (var rb in m_boneRigidbodies.Values)
            {
                float dist = Vector3.Distance(rb.position, point);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = rb;
                }
            }

            return closest;
        }

        private void DistributeForceToNeighbors(Rigidbody hitRb, Vector3 force)
        {
            foreach (var rb in m_boneRigidbodies.Values)
            {
                if (rb == hitRb) continue;

                float distance = Vector3.Distance(rb.position, hitRb.position);
                if (distance < 1f)
                {
                    float falloff = 1f - (distance / 1f);
                    rb.AddForce(force * falloff, ForceMode.Impulse);
                }
            }
        }

        #endregion

        #region LOD Management

        public void SetLODState(RagdollLODState newState, RagdollLODConfig config)
        {
            if (m_currentLODState == newState) return;
            if (!m_physicsEnabled) return;

            m_currentLODState = newState;

            switch (newState)
            {
                case RagdollLODState.FullPhysics:
                    EnableFullPhysics(config);
                    break;

                case RagdollLODState.FrozenPhysics:
                    FreezePhysics(config);
                    break;

                case RagdollLODState.Disabled:
                    DisablePhysics();
                    break;
            }
        }

        private void EnableFullPhysics(RagdollLODConfig config)
        {
            if (m_physicsComponents != null && m_physicsComponents.AreComponentsStripped)
            {
                m_physicsComponents.RestorePhysicsComponents();
            }

            foreach (var rb in m_boneRigidbodies.Values)
            {
                if (rb == null) continue;

                rb.isKinematic = false;
                rb.useGravity = true;
                rb.detectCollisions = true;
                rb.WakeUp();
                rb.solverIterations = config.ActiveSolverIterations;
            }

            foreach (var col in m_boneColliders.Values)
            {
                if (col != null) col.enabled = true;
            }

            foreach (var joint in m_boneJoints.Values)
            {
                if (joint != null) joint.enableProjection = true;
            }

            SetRenderersEnabled(true);
        }


        private void FreezePhysics(RagdollLODConfig config)
        {
            if (config.StripPhysicsWhenFrozen && m_physicsComponents != null)
            {
                m_physicsComponents.StripPhysicsComponents();
                SetRenderersEnabled(true);
                return;
            }

            foreach (var rb in m_boneRigidbodies.Values)
            {
                if (rb == null) continue;

                rb.useGravity = false;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;

                if (config.ReduceIterationsWhenFrozen)
                {
                    rb.solverIterations = config.FrozenSolverIterations;
                }
            }

            foreach (var rb in m_boneRigidbodies.Values)
            {
                if (rb != null)
                {
                    rb.detectCollisions = false;
                }
            }

            foreach (var joint in m_boneJoints.Values)
            {
                if (joint != null) joint.enableProjection = false;
            }

            SetRenderersEnabled(true);
        }

        private void DisablePhysics()
        {
            foreach (var rb in m_boneRigidbodies.Values)
            {
                if (rb == null) continue;

                rb.useGravity = false;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
                rb.detectCollisions = false;
            }

            foreach (var col in m_boneColliders.Values)
            {
                if (col != null) col.enabled = false;
            }

            SetRenderersEnabled(false);
        }


        private void SetRenderersEnabled(bool enabled)
        {
            foreach (var renderer in m_renderers)
            {
                if (renderer != null) renderer.enabled = enabled;
            }
        }

        #endregion

        #region Fade Out

        public void StartFadeOut()
        {
            if (m_isFadingOut) return;

            m_isFadingOut = true;
            m_fadeStartTime = Time.time;
        }

        public void UpdateFadeOut()
        {
            if (!m_isFadingOut) return;

            float fadeProgress = (Time.time - m_fadeStartTime) / m_fadeDuration;

            if (fadeProgress >= 1f)
            {
                SetRenderersEnabled(false);
                return;
            }

            float alpha = 1f - fadeProgress;

            foreach (var renderer in m_renderers)
            {
                if (renderer == null) continue;

                foreach (var mat in renderer.materials)
                {
                    if (mat.HasProperty("_BaseColor"))
                    {
                        Color color = mat.GetColor("_BaseColor");
                        color.a = alpha;
                        mat.SetColor("_BaseColor", color);
                    }
                    else if (mat.HasProperty("_Color"))
                    {
                        Color color = mat.GetColor("_Color");
                        color.a = alpha;
                        mat.SetColor("_Color", color);
                    }
                }
            }
        }

        #endregion

        #region Public API

        public void ResetForPooling()
        {
            StopAllCoroutines();

            m_isInitialized = false;
            m_isFadingOut = false;
            m_physicsEnabled = false;
            m_currentLODState = RagdollLODState.FullPhysics;

            RestoreOriginalBonePose();

            foreach (var rb in m_boneRigidbodies.Values)
            {
                if (rb == null) continue;

                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }

            foreach (var col in m_boneColliders.Values)
            {
                if (col != null) col.enabled = true;
            }

            SetRenderersEnabled(true);

            foreach (var renderer in m_renderers)
            {
                if (renderer == null) continue;

                foreach (var mat in renderer.materials)
                {
                    if (mat.HasProperty("_BaseColor"))
                    {
                        Color color = mat.GetColor("_BaseColor");
                        color.a = 1f;
                        mat.SetColor("_BaseColor", color);
                    }
                    else if (mat.HasProperty("_Color"))
                    {
                        Color color = mat.GetColor("_Color");
                        color.a = 1f;
                        mat.SetColor("_Color", color);
                    }
                }
            }

            if (m_physicsComponents != null && m_physicsComponents.AreComponentsStripped)
            {
                m_physicsComponents.RestorePhysicsComponents();
            }
        }


        public bool ShouldDespawn(RagdollLODConfig config)
        {
            if (m_rootBone != null && m_rootBone.position.y < config.DespawnBelowY)
                return true;

            if (m_isFadingOut && (Time.time - m_fadeStartTime) >= m_fadeDuration)
                return true;

            return false;
        }

        #endregion
    }
}
