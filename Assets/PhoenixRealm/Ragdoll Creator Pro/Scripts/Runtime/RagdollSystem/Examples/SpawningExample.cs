using System.Collections;
using UnityEngine;

namespace PhoenixRealm.RagdollCreatorPro.Examples
{
    public class SpawningExample : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private RagdollLODConfig m_lodConfig;
        [SerializeField] private RagdollEntityComponent m_ragdollComponent;
        [SerializeField] private float m_spawnDelay = 1;

        private void Start()
        {
            if (m_lodConfig != null)
            {
                RagdollManager.Instance.SetConfig(m_lodConfig);
            }

            RagdollManager.Instance.SetReferencePoint(Camera.main.transform);

            StartCoroutine(SpawnRagdolls());
        }

        private IEnumerator SpawnRagdolls()
        {
            yield return new WaitForSeconds(m_spawnDelay);

            var ragdoll = Instantiate(m_ragdollComponent, transform.position, Quaternion.identity);
            KillEntity(ragdoll);

            StartCoroutine(SpawnRagdolls());
        }

        private void KillEntity(RagdollEntityComponent entity)
        {
            if (entity == null) return;

            float randomeForceZ = Random.Range(20f, 60f);
            Vector3 hitPoint = entity.transform.position;
            Vector3 hitForce = entity.transform.forward * randomeForceZ + Vector3.up * 15;
            Vector3 hitNormal = entity.transform.forward;

            entity.Die(hitPoint, hitForce, hitNormal);
        }
    }
}
