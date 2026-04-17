using GameAsset.WarpSpeedFX;
using UnityEngine;

namespace Asasingame.Core.Airplane.Runtimes
{
    public class AirplaneSpaceJump : MonoBehaviour
    {
        [SerializeField] private AirplaneController controller;
        [SerializeField] private AirplaneCamera cameraController;
        [SerializeField] private WarpSpeedFXControl warpSpeedFXControl;
        [SerializeField] private Transform spaceJumpGate;

        [Header("Setting")]
        [SerializeField] private float shake_Amplitude;
        [SerializeField] private float shake_Frequency;
        [SerializeField] private float shake_transitionDuration;
        [Space(5)]
        [SerializeField] private float view_MotionBlur;
        [SerializeField] private float motionBlur_transitionDuration;
        [Space(5)]
        [SerializeField] private float view_Fov;
        [SerializeField] private float fov_transitionDuration;


        private void Start()
        {
            Invoke(nameof(DoSpaceJump),5);
            Invoke(nameof(StopSpaceJump), 15);
        }

        public void DoSpaceJump()
        {
            warpSpeedFXControl.Active();
            /* cameraController.ChangeCameraShake(shake_Amplitude, shake_Frequency, shake_transitionDuration);
             cameraController.ChangeViewMotionBlur(view_MotionBlur, motionBlur_transitionDuration);
             cameraController.ChangeCameraFov(view_Fov, fov_transitionDuration);*/
            controller.ActiveSpaceJumpMovement(-spaceJumpGate.forward);
        }

        public void StopSpaceJump()
        {
            warpSpeedFXControl.DeActive();
            /*cameraController.ChangeCameraShake(0, 0, shake_transitionDuration);
            cameraController.ChangeViewMotionBlur(0, motionBlur_transitionDuration);
            cameraController.ChangeCameraFov(cameraController.DefaultFov, fov_transitionDuration);*/
            controller.DeActiveSpaceJumpMovement();
        }
    }
}


