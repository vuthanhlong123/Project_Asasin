using UnityEngine;
using Cinemachine;
using static Asasingame.Core.Airplane.Runtimes.AirplaneController;
using UnityEngine.InputSystem;

namespace Asasingame.Core.Airplane.Runtimes
{
    public class AirplaneCamera : MonoBehaviour
    {
        private CinemachineBrain brain;

        [Header("References")]
        [SerializeField] private AirplaneController airPlaneController;
        [SerializeField] private CinemachineFreeLook freeLook;
        [SerializeField] private CinemachineVirtualCamera aimCamera;
        [SerializeField] private PlayerInput playerInput;

        [Header("Camera values")]
        [SerializeField] private float cameraDefaultFov = 60f;
        [SerializeField] private float cameraTurboFov = 40f;

        public bool IsAimming => aimCamera.gameObject.activeSelf;

        private void OnEnable()
        {
            airPlaneController.crashAction += Crash;
            playerInput.actions["Aim"].performed += AirplaneCamera_performed;
        }
       
        private void AirplaneCamera_performed(InputAction.CallbackContext obj)
        {
            aimCamera.gameObject.SetActive(!aimCamera.gameObject.activeSelf);
        }

        private void OnDisable()
        {
            airPlaneController.crashAction -= Crash;
            playerInput.actions["Aim"].performed -= AirplaneCamera_performed;
        }

        private void Start()
        {
            brain = GetComponent<CinemachineBrain>();

            //Lock and hide mouse
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            CameraFovUpdate();
        }

        private void CameraFovUpdate()
        {
            //Turbo
            if(!airPlaneController.PlaneIsDead() && airPlaneController.airplaneState == AirplaneState.Flying)
            {
                if (Input.GetKey(KeyCode.LeftShift) && !airPlaneController.TurboOverheating())
                {
                    ChangeCameraFov(cameraTurboFov);
                }
                else
                {
                    ChangeCameraFov(cameraDefaultFov);
                }
            }
            else
            {
                ChangeCameraFov(cameraDefaultFov);
            }
        }

        public void ChangeCameraFov(float _fov)
        {
            float _deltatime = Time.deltaTime * 100f;
            freeLook.m_Lens.FieldOfView = Mathf.Lerp(freeLook.m_Lens.FieldOfView, _fov, 0.05f * _deltatime);
        }

        private void Crash()
        {
            //Change update method after crash
            brain.m_BlendUpdateMethod = CinemachineBrain.BrainUpdateMethod.FixedUpdate;
        }
    }
}