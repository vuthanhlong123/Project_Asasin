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
            if (IsOppositeDirection(transform.forward, direction)) return;

            warpSpeedFXControl.Active(warpFXCompleted);
            controller.ActiveSpaceJumpMovement(direction);
        }

        public bool IsOppositeDirection(Vector3 a, Vector3 b)
        {
            Vector3 normA = a.normalized;
            Vector3 normB = b.normalized;

            float dot = Vector3.Dot(normA, normB);

            return dot <= 0f;
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


