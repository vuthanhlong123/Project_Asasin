using Game.Runtimes.Player;
using UnityEngine;

namespace Game.Runtimes.Managers
{
    public class SpaceShipPlayerManager : Manager
    {
        public static SpaceShipPlayerManager instance;

        [SerializeField] private GameObject sample_Player;

        private SpaceShipPlayer createdPlayer;

        public SpaceShipPlayer Player => createdPlayer;

        private void Awake()
        {
            instance = this;
        }

        public void CreatePlayer(Transform spawnPoint)
        {
            GameObject playerInstance = Instantiate(sample_Player, spawnPoint.position, spawnPoint.rotation, this.transform);
            createdPlayer = playerInstance.GetComponent<SpaceShipPlayer>();
        }
    }
}

