using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Runtimes.Managers
{
    public class GameSceneManager : Manager
    {
        public static GameSceneManager instance;

        private void Awake()
        {
            instance = this;
        }

        public async void LoadSceneAsync(string sceneName, LoadSceneMode mode, Action complete = null)
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, mode);
            while (!operation.isDone)
            {
                await Task.Yield();
            }

            complete?.Invoke();
        }
    }
}


