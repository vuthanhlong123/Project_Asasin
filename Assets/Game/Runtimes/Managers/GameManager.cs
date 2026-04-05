using UnityEngine;

namespace Game.Runtimes.Managers
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance;

        [Header("Members")]
        [SerializeField] private Manager[] managers;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public T GetManager<T>()
        {
            foreach (var manager in managers)
            {
                if (manager is T typedManager)
                {
                    return typedManager;
                }
            }

            return default;
        }
    }
}


