using UnityEngine;

namespace Asasingame.Core.Runtimes.Camera
{
    public class CameraShake : MonoBehaviour
    {
        [SerializeField] float duration = 0.5f;   
        [SerializeField] float magnitude = 0.3f;  

        private Transform camTransform;
        private Vector3 originalPos;

        void Awake()
        {
            camTransform = GetComponent<Transform>();
        }

        public void Shake()
        {
            StopAllCoroutines();
            StartCoroutine(ShakeCoroutine());
        }

        private System.Collections.IEnumerator ShakeCoroutine()
        {
            originalPos = camTransform.localPosition;

            float elapsed = 0.0f;

            while (elapsed < duration)
            {
                float x = Random.Range(-1f, 1f) * magnitude;
                float y = Random.Range(-1f, 1f) * magnitude;

                camTransform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);

                elapsed += Time.deltaTime;

                yield return null;
            }

            camTransform.localPosition = originalPos;
        }
    }
}


