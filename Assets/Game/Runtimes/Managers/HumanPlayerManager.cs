using System.ComponentModel;
using UnityEngine;

namespace Game.Runtimes.Managers
{
    public class HumanPlayerManager : MonoBehaviour
    {
        public static HumanPlayerManager Instance;

        [SerializeField] private GameObject fpsManager;
        [SerializeField] private GameObject sample_Player;

        private GameObject currentPlayer;
        private bool playerStartUped = false;

        private void Awake()
        {
            Instance = this;
        }

        public GameObject CreatePlayerStartUp(Transform spawnPoint)
        {
            if (playerStartUped) return null;

            fpsManager.SetActive(true);
            currentPlayer = Instantiate(sample_Player, spawnPoint.position, spawnPoint.rotation, fpsManager.transform);
            Debug.Log(currentPlayer.transform.position);

            playerStartUped = true;

            return currentPlayer;
        }

        public void CreatePlayer(Transform spawnPoint)
        {
            fpsManager.SetActive(true);
            currentPlayer = Instantiate(sample_Player, spawnPoint.position, spawnPoint.rotation, fpsManager.transform);
        }

        public void RemovePlayer()
        {
            if (currentPlayer != null)
            {
                Destroy(currentPlayer);
            }

            fpsManager.SetActive(false);
        }
    }
}


