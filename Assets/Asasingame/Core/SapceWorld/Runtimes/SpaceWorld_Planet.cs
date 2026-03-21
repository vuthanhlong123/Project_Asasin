using Unity.Mathematics;
using UnityEngine;

namespace Asasingame.Core.SpaceWorld.Runtimes
{
    public class SpaceWorld_Planet : MonoBehaviour
    {
        [SerializeField] private float maxDistance;
        [SerializeField] private float minDistance;
        [SerializeField] private float maxScale;
        [SerializeField] private float minScale;

        private Camera mainCam;
        private Vector3 defaultPos;

        void Start()
        {
            mainCam = Camera.main;
            defaultPos = transform.position;

        }

        private void LateUpdate()
        {
            Vector3 direction = mainCam.transform.position - defaultPos;
            float distance = direction.magnitude;

            if (distance > maxDistance)
            {
                transform.position = mainCam.transform.position + (-direction.normalized * maxDistance);
            }
            else
            {
                transform.position = defaultPos;
            }

            float scale = Mathf.Lerp(minScale, maxScale, maxScale / (distance));
            transform.localScale = new Vector3(scale, scale, scale);
        }
    }
}


