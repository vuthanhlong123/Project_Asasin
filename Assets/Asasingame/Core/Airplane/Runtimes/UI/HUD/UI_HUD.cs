using TMPro;
using UnityEngine;

namespace Asasingame.Core.Airplane.Runtimes.UIs
{
    public class UI_HUD : MonoBehaviour
    {
        [SerializeField] private AirplaneCamera cameraController;
        [SerializeField] private GameObject container;

        [Header("Pitch Ladder")]
        [SerializeField] private RectTransform rect_ContentPitchLadder;
        [SerializeField] private RectTransform rect_PitchLadder;
        [SerializeField] private Vector2 pitchLadder_ScrollRange;

        [Header("Speed Ladder")]
        [SerializeField] private RectTransform rect_SpeedCountFrame;
        [SerializeField] private TextMeshProUGUI text_SpeedCount;
        [SerializeField] private Vector2 speedCountFrame_SlideRange;

        [Header("Altitude")]
        [SerializeField] private RectTransform rect_AltitudeCountFrame;
        [SerializeField] private TextMeshProUGUI text_AltitudeCount;
        [SerializeField] private Vector2 altitudeCountFrame_SlideRange;
        [SerializeField] private float maxAltitude;

        [Header("Compass")]
        [SerializeField] private Material material;
        [SerializeField] private float defaultScrollValue;

        private Camera cam;
        private AirplaneController airplaneController;

        void Start()
        {
            airplaneController = GetComponentInParent<AirplaneController>();
            cam = Camera.main;
        }

        void LateUpdate()
        {
            UpdateSpeedLadder();
            UpdateAltitude();
            UpdatePitchLadder();
            UpdateCompass();
        }
        
        private void UpdatePitchLadder()
        {
            if (cam == null) return;

            //Rotation
            Quaternion camRot = cam.transform.rotation;
            Vector3 euler = camRot.eulerAngles;
            euler.z = 0;
            rect_ContentPitchLadder.rotation = Quaternion.Euler(euler);

            Vector3 a1 = transform.forward;
            Vector3 a2 = a1;
            a2.y = 0;

            //Scroll
            float angle = Vector3.SignedAngle(a1, a2, Vector3.up);
            if(Vector3.Angle(transform.forward, Vector3.up)>90)
            {
                angle = -angle;
            }

            rect_PitchLadder.anchoredPosition = new Vector2(0, Mathf.Lerp(pitchLadder_ScrollRange.x, pitchLadder_ScrollRange.y, (angle +90)/180));
        }

        private void UpdateSpeedLadder()
        {
            if(airplaneController == null) return;
            text_SpeedCount.text = Mathf.Round(airplaneController.CurrentSpeed()).ToString();

            rect_SpeedCountFrame.anchoredPosition = new Vector2(0, Mathf.Lerp(speedCountFrame_SlideRange.x, speedCountFrame_SlideRange.y, airplaneController.PercentToMaxSpeed()));
        }

        private void UpdateAltitude()
        {
            if (airplaneController == null) return;

            text_AltitudeCount.text = Mathf.Round(airplaneController.transform.position.y).ToString();
            rect_AltitudeCountFrame.anchoredPosition = new Vector2(0, Mathf.Lerp(altitudeCountFrame_SlideRange.x, altitudeCountFrame_SlideRange.y, airplaneController.transform.position.y/maxAltitude));
        }

        private void UpdateCompass()
        {
            material.SetFloat("_Scroll", defaultScrollValue + transform.eulerAngles.y * ((float)1 /360));
        }

        private void OnEnable()
        {
            cameraController.EnableAimCamera += CameraController_EnableAimCamera;
            cameraController.EnableFreeLookCamera += CameraController_EnableFreeLookCamera;
        }

        private void CameraController_EnableFreeLookCamera()
        {
            container.SetActive(false);
        }

        private void CameraController_EnableAimCamera()
        {
            container.SetActive(true);
        }

        private void OnDisable()
        {
            cameraController.EnableAimCamera -= CameraController_EnableAimCamera;
            cameraController.EnableFreeLookCamera -= CameraController_EnableFreeLookCamera;
        }

    }
}


