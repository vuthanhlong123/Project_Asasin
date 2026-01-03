using UnityEngine;

namespace Asasingame.Core.Object
{
    public class ObjectAddMovement : MonoBehaviour
    {
        [SerializeField] private float startDelayTime;
        [SerializeField] private float duration;
        [SerializeField] private float distance;
        [SerializeField] private Vector3 direction;
        [SerializeField] private AnimationCurve curve;

        private float timeCount;
        private Vector3 startPoistion;

        private void Start()
        {
            startPoistion = transform.position;
            if (startDelayTime > 0)
            {
                this.enabled = false;
                Invoke(nameof(Active), startDelayTime);
            }
        }

        private void Update()
        {
            timeCount += Time.deltaTime;

            float t = curve.Evaluate(timeCount / duration);
            Vector3 delta = Vector3.Lerp(Vector3.zero, direction * distance, t);
            transform.position = new Vector3(transform.position.x, startPoistion.y + delta.y, transform.position.z);

            if (timeCount >= duration)
            {
                this.enabled = false;
            }
        }

        private void Active()
        {
            this.enabled = true;
        }
    }
}


