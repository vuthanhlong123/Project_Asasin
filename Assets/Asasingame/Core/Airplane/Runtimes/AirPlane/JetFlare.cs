using UnityEngine;

namespace Asasingame.Core.Airplane.Runtimes
{
    public class JetFlare : MonoBehaviour
    {
        [SerializeField] private float resistance;

        private float speed;
        private float remainingSpeed;
        private Vector3 direction;

        private void Start()
        {
            remainingSpeed = speed;
        }

        private void Update()
        {
            if (remainingSpeed > 0)
            {
                transform.position += direction * remainingSpeed * Time.deltaTime;
                transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
                remainingSpeed -= resistance * Time.deltaTime;
            }
        }

        public void SetValue(Vector3 direction, float speed)
        {
            this.direction = direction;
            this.speed = speed;
        }
    }
}


