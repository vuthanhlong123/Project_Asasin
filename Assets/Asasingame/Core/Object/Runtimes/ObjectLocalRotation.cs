using UnityEngine;

namespace Asasingame.Core.Object
{
    public class ObjectLocalRotation : MonoBehaviour
    {
        [SerializeField] private float startDelayTime;
        [SerializeField] private float duration;
        [SerializeField] private Vector3 addEuler;
        [SerializeField] private AnimationCurve curve;

        private float timeCount;
        private Vector3 startEuler;

        private void Start()
        {
            if (startDelayTime > 0)
            {
                this.enabled = false;
                Invoke(nameof(Active), startDelayTime);
            }
            else
            {
                startEuler = transform.localEulerAngles;
            }
        }

        private void Update()
        {
            timeCount += Time.deltaTime;

            float t = curve.Evaluate(timeCount / duration);
            transform.localEulerAngles = Vector3.Lerp(startEuler, startEuler + addEuler, t);

            if (timeCount >= duration)
            {
                this.enabled = false;
            }
        }

        private void Active()
        {
            startEuler = transform.localEulerAngles;
            this.enabled = true;
        }
    }
}


