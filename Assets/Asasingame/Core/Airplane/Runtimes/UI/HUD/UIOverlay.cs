using TMPro;
using UnityEngine;

namespace Asasingame.Core.Airplane.Runtimes.UIs
{

    public class UIOverlay : MonoBehaviour
    {
        [SerializeField] private AirplaneController controller;
        [SerializeField] private AirplaneCamera cameraController;
        [SerializeField] private GameObject container;
        [SerializeField] private TextMeshProUGUI text_Speed;
        [SerializeField] private TextMeshProUGUI text_Altitude;
        [SerializeField] private RectTransform rect_Reticle;

        [Header("Compass")]
        [SerializeField] private Material material;
        [SerializeField] private float defaultScrollValue;

        private Camera mainCamera;

        private void Start()
        {
            mainCamera = Camera.main;
        }

        private void LateUpdate()
        {
            text_Speed.text = Mathf.Round(controller.CurrentSpeed()).ToString();
            text_Altitude.text = Mathf.Round(controller.transform.position.y).ToString();

            rect_Reticle.transform.position = mainCamera.WorldToScreenPoint(controller.transform.position + controller.transform.forward * 200);

            UpdateCompass();
        }

        private void UpdateCompass()
        {
            material.SetFloat("_Scroll", defaultScrollValue + controller.transform.eulerAngles.y * ((float)1 / 360));
        }

        private void OnEnable()
        {
            cameraController.EnableAimCamera += CameraController_EnableAimCamera;
            cameraController.EnableFreeLookCamera += CameraController_EnableFreeLookCamera;
        }

        private void CameraController_EnableFreeLookCamera()
        {
            container.SetActive(true);
        }

        private void CameraController_EnableAimCamera()
        {
            container.SetActive(false);
        }

        private void OnDisable()
        {
            cameraController.EnableAimCamera -= CameraController_EnableAimCamera;
            cameraController.EnableFreeLookCamera -= CameraController_EnableFreeLookCamera;
        }
    }
}

