using Asasingame.Core.Airplane.Runtimes;
using Game.Runtimes.Managers;
using System;
using UnityEngine;

namespace Game.Runtimes.SpaceJump
{
    public class SpaceJumpGate : MonoBehaviour
    {
        [SerializeField] private string targetScene;

        public string TargetScene => targetScene;

        private bool isAvailable;

        private void Start()
        {
            Invoke(nameof(GateAvailable), 10);
        }

        private void GateAvailable()
        {
            isAvailable = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isAvailable) return;

            AirplaneSpaceJump spaceJumper = other.GetComponent<AirplaneSpaceJump>();
            if (spaceJumper == null) return;

            spaceJumper.DoSpaceJump(transform.forward, warpFXCompleted: () =>
            {
                MoveToNewWorld(() =>
                {
                    SpaceJumpGate targetGate = SpaceJumpManager.instance.GetConnectGate();
                    if (targetGate != null)
                    {
                        spaceJumper.MoveToTargetJump(targetGate.transform);
                    }
                    spaceJumper.StopSpaceJump(2);
                });
            });
        }

        private void MoveToNewWorld(Action completed = null)
        {
            GameSceneManager.instance.ChangeAdditiveScene(targetScene,completed);
        }
    }
}


