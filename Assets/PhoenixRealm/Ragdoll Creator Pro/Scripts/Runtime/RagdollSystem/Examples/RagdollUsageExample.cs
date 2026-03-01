using UnityEngine;
using UnityEngine.SceneManagement;

namespace PhoenixRealm.RagdollCreatorPro.Examples
{
    public class RagdollUsageExample : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private RagdollLODConfig m_lodConfig;

        [Header("Weapon Settings")]
        [SerializeField] private float m_gunForce = 500f;
        [SerializeField] private float m_gunUpwardForce = 100f;
        [SerializeField] private LayerMask m_hitLayers = ~0;

        [Header("Debug")]
        [SerializeField] private bool m_showDebugRays = true;
        [SerializeField] private float m_debugRayDuration = 2f;

        private void Start()
        {
            if (m_lodConfig != null)
            {
                RagdollManager.Instance.SetConfig(m_lodConfig);
            }

            RagdollManager.Instance.SetReferencePoint(Camera.main.transform);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }

            if (Input.GetMouseButtonDown(0))
            {
                RaycastForHit();
            }
        }

        private void RaycastForHit()
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (m_showDebugRays)
            {
                Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, m_debugRayDuration);
            }

            if (!Physics.Raycast(ray, out var hit, Mathf.Infinity, m_hitLayers))
                return;

            if (m_showDebugRays)
            {
                Debug.DrawLine(ray.origin, hit.point, Color.green, m_debugRayDuration);
                Debug.DrawRay(hit.point, hit.normal * 0.5f, Color.blue, m_debugRayDuration);
            }

            var entity = FindEntityFromHit(hit);
            if (entity != null)
            {
                KillEntity(entity, hit);
            }
        }

        private RagdollEntityComponent FindEntityFromHit(RaycastHit hit)
        {
            if (!hit.collider)
            {
                Debug.LogWarning("[Ragdoll Usage] Hit collider is null");
                return null;
            }

            // Strategy 1: Check the hit object itself
            var entity = hit.collider.GetComponent<RagdollEntityComponent>();
            if (entity != null)
            {
                Debug.Log($"[Ragdoll Usage] Found entity on hit object: {hit.collider.name}");
                return entity;
            }

            // Strategy 2: Traverse up the parent hierarchy (most common for bones)
            entity = hit.collider.GetComponentInParent<RagdollEntityComponent>();
            if (entity != null)
            {
                Debug.Log($"[Ragdoll Usage] Found entity in parent hierarchy from: {hit.collider.name}");
                return entity;
            }

            // Strategy 3: Check attached rigidbody
            var rb = hit.rigidbody;
            if (rb != null)
            {
                entity = rb.GetComponent<RagdollEntityComponent>();
                if (entity != null)
                {
                    Debug.Log($"[Ragdoll Usage] Found entity on attached rigidbody: {rb.name}");
                    return entity;
                }

                // Also check rigidbody's parent
                entity = rb.GetComponentInParent<RagdollEntityComponent>();
                if (entity != null)
                {
                    Debug.Log($"[Ragdoll Usage] Found entity in rigidbody parent: {rb.name}");
                    return entity;
                }
            }

            // Strategy 4: Check root transform
            if (hit.transform != null)
            {
                var root = hit.transform.root;
                entity = root.GetComponent<RagdollEntityComponent>();
                if (entity != null)
                {
                    Debug.Log($"[Ragdoll Usage] Found entity on root: {root.name}");
                    return entity;
                }
            }

            Debug.LogWarning($"[Ragdoll Usage] No entity found from hit on: {hit.collider.name}");
            return null;
        }

        private void KillEntity(RagdollEntityComponent entity, RaycastHit hit)
        {
            if (entity == null) return;

            Vector3 hitPoint = hit.point;
            Vector3 shootDirection = (hit.point - Camera.main.transform.position).normalized;
            Vector3 hitForce = shootDirection * m_gunForce + Vector3.up * m_gunUpwardForce;
            Vector3 hitNormal = hit.normal;
            Transform hitBone = hit.collider.transform;

            entity.Die(hitPoint, hitForce, hitNormal, hitBone);
        }

        private void OnGUI()
        {
            if (!m_showDebugRays) return;

            GUI.Box(new Rect(10, 150, 250, 120), "Shooting Controls");
            GUI.Label(new Rect(20, 175, 230, 20), "Left Click - Shoot");
            GUI.Label(new Rect(20, 200, 230, 20), $"Gun Force: {m_gunForce}");
            GUI.Label(new Rect(20, 225, 230, 20), $"R To Reset Scene");
        }
    }
}
