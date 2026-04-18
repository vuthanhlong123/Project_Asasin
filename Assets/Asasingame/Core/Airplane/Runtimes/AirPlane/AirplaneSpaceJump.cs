using GameAsset.WarpSpeedFX;
using System;
using UnityEngine;

namespace Asasingame.Core.Airplane.Runtimes
{
    public class AirplaneSpaceJump : MonoBehaviour
    {
        [SerializeField] private AirplaneController controller;
        [SerializeField] private WarpSpeedFXControl warpSpeedFXControl;

        public void DoSpaceJump(Vector3 direction, Action warpFXCompleted = null)
        {
            warpSpeedFXControl.Active(warpFXCompleted);
            controller.ActiveSpaceJumpMovement(direction);
        }

        public void StopSpaceJump(float delay = 0f)
        {
            if(delay == 0)
            {
                DoStopSpaceJump();
            }
            else
            {
                Invoke(nameof(DoStopSpaceJump), delay);
            }
        }

        private void DoStopSpaceJump()
        {
            warpSpeedFXControl.DeActive(() =>
            {
                controller.DeActiveSpaceJumpMovement();
            });
        }

        public void MoveToTargetJump(Transform target)
        {
            controller.SpaceJumpToTargetMovement(-target.forward, target);
        }
    }
}


