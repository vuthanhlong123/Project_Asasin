using UnityEngine;

namespace Asasingame.Core.Airplane.Runtimes
{
    public class AirplaneProjectile : MonoBehaviour
    {
        [SerializeField] private float lifeTime;
        [SerializeField] private float force;
        [SerializeField] private Rigidbody rigi;

        private void Start()
        {
            rigi.AddForce(gameObject.transform.forward * force, ForceMode.Force);
            Invoke(nameof(DestroySelf), lifeTime);
        }

        private void DestroySelf()
        {
            Destroy(gameObject);
        }
    }
}


