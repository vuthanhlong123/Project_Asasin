using UnityEngine;

namespace Asasingame.Core.SpaceWorld
{
    public class SpaceWorld_PlanetScaler : MonoBehaviour
    {
        [SerializeField] private Vector2 scaleRange;
        [SerializeField] private Vector2 distanceRange;

        private Camera _mainCamera;

        private void Start()
        {
            _mainCamera = Camera.main;
        }

        private void LateUpdate()
        {
            if (_mainCamera == null) return;

            float distanceToCamera = (_mainCamera.transform.position - transform.position).magnitude;
            if (distanceToCamera > distanceRange.y)
            {
                transform.localScale = new Vector3(scaleRange.x, scaleRange.x, scaleRange.x);
                return;
            }
            if (distanceToCamera < distanceRange.x)
            {
                transform.localScale = new Vector3(scaleRange.y, scaleRange.y, scaleRange.y);
                return;
            }
            float scaleValue = Mathf.Lerp(scaleRange.y, scaleRange.x, (distanceToCamera - distanceRange.x) / (distanceRange.y - distanceRange.x));
            float valueNormalize = Mathf.Clamp(scaleValue, scaleRange.x, scaleRange.y);
            transform.localScale = new Vector3(scaleValue, scaleValue, scaleValue);
        }
    }
}


