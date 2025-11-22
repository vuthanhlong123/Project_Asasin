using UnityEngine;
using UnityEngine.InputSystem;

namespace Asasingame.Core.Runtimes.Input
{
    public class PlayerInputManager : MonoBehaviour
    {
        public static PlayerInputManager Instance;

        [SerializeField] private PlayerInput PlayerInput;

        public static PlayerInput Input;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);

                Input = PlayerInput;
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}


