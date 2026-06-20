using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Runtimes.Input
{
    public class GameInputManager : MonoBehaviour
    {
        public static GameInputManager instance;

        [SerializeField] private PlayerInput gamePlayInput;

        private void Awake()
        {
            instance = this;

            Debug.Log(GetInputKey("Interaction"));
        }

        public float GetInputAxis(string name)
        {
            return gamePlayInput.actions[name].ReadValue<float>();
        }

        public string GetInputKey(string name)
        {
            return gamePlayInput.actions[name].GetBindingDisplayString();
        }
    }
}


