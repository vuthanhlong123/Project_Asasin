using UnityEngine;
using UnityEngine.Events;

namespace Game.Runtimes.FPS
{
    public class EndLadderOnTopHandler : MonoBehaviour
    {
        private Vector3 target;
        private Vector3 startPosition;

        private float duration;
        private float executedTime;

        private UnityAction _completed;

        private void Start()
        {
            startPosition = transform.position;
        }

        public void SetValue(Vector3 targetPosition, float duration, UnityAction completed = null)
        {
            target = targetPosition;
            this.duration = duration;
            _completed = completed;
        }

        private void Update()
        {
            executedTime += Time.deltaTime;
            transform.position = Vector3.Lerp(startPosition, target, executedTime/duration);

            if (executedTime >= duration)
            {
                _completed?.Invoke();
                Destroy(this);
            }
        }
    }
}


