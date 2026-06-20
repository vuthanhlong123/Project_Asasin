using Game.Runtimes.Managers;
using UnityEngine;

namespace Game.Runtimes.Station
{
    public class StationController : MonoBehaviour
    {
        [SerializeField] private TDScene tdScene;
        [SerializeField] private Transform wakeUpSpawnPoint;
        [SerializeField] private Transform moveInSpawnPoint;
        [SerializeField] private bool isTest;

        public Transform WakeUpSpawnPoint => wakeUpSpawnPoint;
        public Transform MoveInSpawnPoint => moveInSpawnPoint;

        private void Start()
        {
            if(HumanPlayerManager.Instance)
            {
                var player = HumanPlayerManager.Instance.CreatePlayerStartUp(!isTest ? wakeUpSpawnPoint : moveInSpawnPoint);
                if (player != null)
                {
                    tdScene.SetPlayerChar(player);
                }
            }
        }
    }
}


