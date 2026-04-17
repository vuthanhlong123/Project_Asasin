using Cinemachine;
using System;
using System.Collections;
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

        private float shakeFrequency;
        private float shakeAmplitude;
        private float viewMotionBlur;
        private float viewFOV;

        private Coroutine coroutine_ChangeCameraShakeValue;
        private Coroutine coroutine_ChangeViewMotionBlurValue;
        private Coroutine coroutine_ChangeCameraFovValue;

        public bool IsAimming => aimCamera.gameObject.activeSelf;
        public float DefaultFov => cameraDefaultFov;
        public float ShakeFrequency => shakeFrequency;
        public float ShakeAmplitude => shakeAmplitude;
        public float ViewMotionBlur => viewMotionBlur;
        public float ViewFOV => viewFOV;

        public event UnityAction EnableFreeLookCamera;
        public void OnEnableFreeLookCamera() => EnableFreeLookCamera?.Invoke();

        public event UnityAction EnableAimCamera;
        public void OnEnableAimCamera() => EnableAimCamera?.Invoke();


        private void OnEnable()
        {
            airPlaneController.crashAction += Crash;
            playerInput.actions["Aim"].performed += AirplaneCamera_performed;

            viewFOV = cameraDefaultFov;
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

        private void LateUpdate()
        {
            CameraFovUpdate();
        }

        private void CameraFovUpdate()
        {
            ChangeShake(shakeAmplitude, shakeFrequency);
            ChangeMotionBlur(viewMotionBlur);
            ChangeCameraFov(viewFOV);

            //Turbo
            if (!airPlaneController.PlaneIsDead() && airPlaneController.airplaneState == AirplaneState.Flying)
            {
                if (Input.GetKey(KeyCode.LeftShift) && !airPlaneController.TurboOverheating())
                {
                    //ChangeCameraFov(cameraTurboFov);
                    //ChangeShake(amplitudeRange.y, frequencyRange.y);
                    //ChangeMotionBlur(motionBlur_max);
                }
                else
                {
                    //ChangeCameraFov(cameraDefaultFov);
                    //ChangeShake(amplitudeRange.x, frequencyRange.x);
                    //ChangeMotionBlur(motionBlur_min);

                }
            }
            else
            {
                //ChangeCameraFov(cameraDefaultFov);
                //ChangeShake(amplitudeRange.x, frequencyRange.x);
                //ChangeMotionBlur(motionBlur_min);
            }
        }

        private void Crash()
        {
            //Change update method after crash
            brain.m_BlendUpdateMethod = CinemachineBrain.BrainUpdateMethod.FixedUpdate;
        }

        private void ChangeCameraFov(float _fov)
        {
            freeLook.m_Lens.FieldOfView = _fov;

            //float _deltatime = Time.deltaTime * fovChangeSpeed;
            //freeLook.m_Lens.FieldOfView = Mathf.Lerp(freeLook.m_Lens.FieldOfView, _fov, 0.05f * _deltatime);
        }

        public void ChangeCameraFov(float value, float duration)
        {
            if (coroutine_ChangeCameraFovValue != null)
            {
                StopCoroutine(coroutine_ChangeCameraFovValue);
            }

            if(duration > 0)
                StartCoroutine(DoChangeCameraFovValue(value, duration));
            else
            {
                viewFOV = value;
            }
        }

        private IEnumerator DoChangeCameraFovValue(float value, float duration)
        {
            float start = Time.time;
            float startFov = viewFOV;

            while (Time.time - start < duration)
            {
                float delta = (Time.time - start) / duration;
                viewFOV = Mathf.Lerp(startFov, value, delta);
                yield return null;
            }

            viewFOV = value;
        }


        private void ChangeShake(float amplitude, float frequency)
        {
            if (topRigNoise == null || midRigNoise == null || botRigNoise == null)
            {
                return;
            }

            /*  float _deltatime = Time.deltaTime * shake_changeSpeed;

              topRigNoise.m_AmplitudeGain = midRigNoise.m_AmplitudeGain = botRigNoise.m_AmplitudeGain = Mathf.Lerp(topRigNoise.m_AmplitudeGain, amplitude, 0.05f * _deltatime); 
              topRigNoise.m_FrequencyGain = midRigNoise.m_FrequencyGain = botRigNoise.m_FrequencyGain = Mathf.Lerp(topRigNoise.m_FrequencyGain, frequency, 0.05f * _deltatime);*/
            topRigNoise.m_AmplitudeGain = midRigNoise.m_AmplitudeGain = botRigNoise.m_AmplitudeGain = amplitude;
            topRigNoise.m_FrequencyGain = midRigNoise.m_FrequencyGain = botRigNoise.m_FrequencyGain = frequency;
        }

        public void ChangeCameraShake(float amplitude, float frequency, float duration)
        {
            if (coroutine_ChangeCameraShakeValue != null)
            {
                StopCoroutine(coroutine_ChangeCameraShakeValue);
            }

            if(duration > 0) 
                StartCoroutine(DoChangeCameraShakeValue(amplitude, frequency, duration));
            else
            {
                shakeAmplitude = amplitude;
                shakeFrequency = frequency;
            }
        }

        private IEnumerator DoChangeCameraShakeValue(float amplitude, float frequency, float duration)
        {
            float start = Time.time;
            float startAmplitude = shakeAmplitude;
            float startFrequency = shakeFrequency;

            while (Time.time - start < duration)
            {
                float delta = (Time.time - start) / duration;
                shakeAmplitude = Mathf.Lerp(startAmplitude, amplitude, delta);
                shakeFrequency = Mathf.Lerp(startFrequency, frequency, delta);
                yield return null;
            }

            shakeAmplitude = amplitude;
            shakeFrequency = frequency;
        }

        private void ChangeMotionBlur(float value)
        {
            if(motionBlur)
            {
                //float _deltatime = Time.deltaTime * motionBlur_changeSpeed;
                //motionBlur.intensity.Override(Mathf.Lerp(motionBlur.intensity.value, value, 0.05f * _deltatime));

                motionBlur.intensity.Override(value);
            }
        }

        public void ChangeViewMotionBlur(float value, float duration)
        {
            if(coroutine_ChangeViewMotionBlurValue != null)
            {
                StopCoroutine(coroutine_ChangeViewMotionBlurValue);
            }

            if (duration > 0)
                StartCoroutine(DoChangeViewMotionBlur(value, duration));
            else viewMotionBlur = value;
        }

        private IEnumerator DoChangeViewMotionBlur(float value, float duration)
        {
            float start = Time.time;
            float startMotionBlur = viewMotionBlur;

            while (Time.time - start < duration)
            {
                float delta = (Time.time - start) / duration;
                viewMotionBlur = Mathf.Lerp(startMotionBlur, value, delta);
                yield return null;
            }

            viewMotionBlur = value;
        }
    }
}