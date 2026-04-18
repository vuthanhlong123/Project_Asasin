using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Runtimes.Managers
{
    public class GameSceneManager : Manager
    {
        public static GameSceneManager instance;

        [SerializeField] private string startScene;

        private string lastAdditiveSceneName;

        public string LastAdditiveSceneName => lastAdditiveSceneName;

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            LoadSceneAsync(startScene, LoadSceneMode.Additive, complete: () =>
            {
                lastAdditiveSceneName = startScene;
            });
        }

        public static List<Scene> GetLoadedAdditiveScenes()
        {
            List<Scene> additiveScenes = new List<Scene>();

            int sceneCount = SceneManager.sceneCount;
            Scene activeScene = SceneManager.GetActiveScene();

            for (int i = 0; i < sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded && scene != activeScene)
                {
                    additiveScenes.Add(scene);
                }
            }

            return additiveScenes;
        }

        public void ChangeAdditiveScene(string sceneName, Action completed = null)
        {
            var additiveScenes = GetLoadedAdditiveScenes();
            if (additiveScenes.Count == 0)
            {
                LoadSceneAsync(sceneName, LoadSceneMode.Additive, completed);
            }
            else
            {
                lastAdditiveSceneName = additiveScenes[0].name;

                SwitchAdditiveScene(additiveScenes[0].name, sceneName, completed);
            }
        }

        public void LoadSceneAsync(string sceneName, LoadSceneMode mode, Action complete = null)
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, mode);

            operation.completed += (AsyncOperation op) =>
            {
                complete?.Invoke();
            };
        }

        public void SwitchAdditiveScene(string oldSceneName, string newSceneName, Action completed = null)
        {
            if (SceneManager.GetSceneByName(oldSceneName).isLoaded)
            {
                AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(oldSceneName);
                unloadOp.completed += (AsyncOperation op) =>
                {
                    AsyncOperation loadOp = SceneManager.LoadSceneAsync(newSceneName, LoadSceneMode.Additive);
                    loadOp.completed += (AsyncOperation op) =>
                    {
                        completed?.Invoke();
                    };
                };
            }
            else
            {
                AsyncOperation loadOp = SceneManager.LoadSceneAsync(newSceneName, LoadSceneMode.Additive);
                loadOp.completed += (AsyncOperation op) =>
                {
                    completed?.Invoke();
                };
            }
        }
    }
}


