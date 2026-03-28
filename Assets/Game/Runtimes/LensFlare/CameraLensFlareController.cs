using UnityEngine;

namespace Game.Runtimes.LensFlares
{
    public class CameraLensFlareController : MonoBehaviour
    {
        [SerializeField] private UnityEngine.Rendering.LensFlareComponentSRP target;
        [SerializeField] private Light sunLight;
        [SerializeField] private float refreshTime;
        [SerializeField] private float distance = 100;
        [SerializeField] private LayerMask cullingMask;

        private Camera _mainCam;

        private void Start()
        {
            _mainCam = Camera.main;

            InvokeRepeating(nameof(Execute), 0, refreshTime);
        }

        private void Execute()
        {
            if (_mainCam == null) return;

            Vector3 direction = ((sunLight.transform.position + (-sunLight.transform.forward * 100000)) - _mainCam.transform.position);
            Ray ray = new Ray(_mainCam.transform.position, direction.normalized);
            if(Physics.Raycast(ray, out RaycastHit hit, distance, cullingMask))
            {
                target.enabled = false;
            }
            else
            {
                target.enabled = true;
            }
        }

        public void Refresh()
        {
            _mainCam = Camera.main;
            Debug.Log(_mainCam.gameObject.name);
        }
    }
}


