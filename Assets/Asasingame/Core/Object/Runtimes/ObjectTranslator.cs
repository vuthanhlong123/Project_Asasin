using UnityEngine;

namespace Asasingame.Core.Object.Runtimes
{
    public class ObjectTranslator : MonoBehaviour
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
            if (startDelayTime>0)
            {
                this.enabled = false;
                Invoke(nameof(Active), startDelayTime);
            }
            else
            {
                startPoistion = transform.position;
            }
        }

        private void Update()
        {
            timeCount += Time.deltaTime;

            float t = curve.Evaluate(timeCount/duration);
            transform.position = Vector3.Lerp(startPoistion, startPoistion + direction * distance, t);

            if(timeCount >= duration)
            {
                this.enabled = false;
            }
        }

        private void Active()
        {
            startPoistion = transform.position;
            this.enabled = true;
        }
    }
}


