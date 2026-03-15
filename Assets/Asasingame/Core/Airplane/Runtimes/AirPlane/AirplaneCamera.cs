using Cinemachine;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static Asasingame.Core.Airplane.Runtimes.AirplaneController;

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
        [SerializeField] private float fovChangeSpeed = 100f;

        [Header("Motion Blur")]
        [SerializeField] private Volume volume;
        [SerializeField] private float motionBlur_min;
        [SerializeField] private float motionBlur_max;
        [SerializeField] private float motionBlur_changeSpeed;

        private MotionBlur motionBlur;

        [Header("Shake")]
        [SerializeField] private Vector2 frequencyRange;
        [SerializeField] private Vector2 amplitudeRange;
        [SerializeField] private float shake_changeSpeed;

        private CinemachineBasicMultiChannelPerlin topRigNoise;
        private CinemachineBasicMultiChannelPerlin midRigNoise;
        private CinemachineBasicMultiChannelPerlin botRigNoise;

        public bool IsAimming => aimCamera.gameObject.activeSelf;

        public event UnityAction EnableFreeLookCamera;
        public void OnEnableFreeLookCamera() => EnableFreeLookCamera?.Invoke();

        public event UnityAction EnableAimCamera;
        public void OnEnableAimCamera() => EnableAimCamera?.Invoke();


        private void OnEnable()
        {
            airPlaneController.crashAction += Crash;
            playerInput.actions["Aim"].performed += AirplaneCamera_performed;
        }
       
        private void AirplaneCamera_performed(InputAction.CallbackContext obj)
        {
            ChangeAimCameraState();
        }

        private void ChangeAimCameraState()
        {
            aimCamera.gameObject.SetActive(!aimCamera.gameObject.activeSelf);

            if (aimCamera.gameObject.activeSelf)
            {
                OnEnableAimCamera();
            }
            else
            {
                OnEnableFreeLookCamera();
            }
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

            aimCamera.gameObject.SetActive(false);
            OnEnableFreeLookCamera();

            if (volume && volume.profile.TryGet(out motionBlur))
            {
                Debug.Log("Motion Blur found");
            }

            topRigNoise = freeLook.GetRig(0).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            midRigNoise = freeLook.GetRig(1).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            botRigNoise = freeLook.GetRig(2).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
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
                    ChangeShake(amplitudeRange.y, frequencyRange.y);
                    ChangeMotionBlur(motionBlur_max);
                }
                else
                {
                    ChangeCameraFov(cameraDefaultFov);
                    ChangeShake(amplitudeRange.x, frequencyRange.x);
                    ChangeMotionBlur(motionBlur_min);

                }
            }
            else
            {
                ChangeCameraFov(cameraDefaultFov);
                ChangeShake(amplitudeRange.x, frequencyRange.x);
                ChangeMotionBlur(motionBlur_min);
            }
        }

        public void ChangeCameraFov(float _fov)
        {
            float _deltatime = Time.deltaTime * fovChangeSpeed;
            freeLook.m_Lens.FieldOfView = Mathf.Lerp(freeLook.m_Lens.FieldOfView, _fov, 0.05f * _deltatime);
        }

        private void Crash()
        {
            //Change update method after crash
            brain.m_BlendUpdateMethod = CinemachineBrain.BrainUpdateMethod.FixedUpdate;
        }

        public void ChangeShake(float amplitude, float frequency)
        {
            if(topRigNoise == null || midRigNoise == null || botRigNoise == null)
            {
                return;
            }

            float _deltatime = Time.deltaTime * shake_changeSpeed;

            topRigNoise.m_AmplitudeGain = midRigNoise.m_AmplitudeGain = botRigNoise.m_AmplitudeGain = Mathf.Lerp(topRigNoise.m_AmplitudeGain, amplitude, 0.05f * _deltatime); 
            topRigNoise.m_FrequencyGain = midRigNoise.m_FrequencyGain = botRigNoise.m_FrequencyGain = Mathf.Lerp(topRigNoise.m_FrequencyGain, frequency, 0.05f * _deltatime); 
        }

        public void ChangeMotionBlur(float value)
        {
            if(motionBlur)
            {
                float _deltatime = Time.deltaTime * motionBlur_changeSpeed;
                motionBlur.intensity.Override(Mathf.Lerp(motionBlur.intensity.value, value, 0.05f * _deltatime));
            }
        }
    }
}