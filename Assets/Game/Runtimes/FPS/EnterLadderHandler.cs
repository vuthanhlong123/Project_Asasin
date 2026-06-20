using UnityEngine;
using UnityEngine.Events;

namespace Game.Runtimes.FPS
{
    public class EnterLadderHandler : MonoBehaviour
    {
        private Vector3 targetPosition;
        private Vector3 startPosition;
        private Quaternion targetRotation;
        private Quaternion startRotation;

        private float duration;
        private float executedTime;

        private CustomFPSController fpsController;
        private UnityAction _completed;

        private void Start()
        {
            startPosition = transform.position;
            startRotation = fpsController.cameraTransform.rotation;
        }

        public void SetValue(Vector3 targetPosition, Vector3 direction, float duration, CustomFPSController fpsController, UnityAction completed = null)
        {
            this.targetPosition = targetPosition;
            this.targetRotation = Quaternion.LookRotation(direction);
            this.duration = duration;
            this.fpsController = fpsController;
            _completed = completed;
        }

        private void Update()
        {
            executedTime += Time.deltaTime;
            transform.position = Vector3.Lerp(startPosition, targetPosition, executedTime / duration);
            fpsController.cameraTransform.rotation = Quaternion.Slerp(startRotation, targetRotation, executedTime / duration);

            if (executedTime >= duration)
            {
                fpsController.RestCameraControlProperty();
                _completed?.Invoke();
                Destroy(this);
            }
        }
    }
}


