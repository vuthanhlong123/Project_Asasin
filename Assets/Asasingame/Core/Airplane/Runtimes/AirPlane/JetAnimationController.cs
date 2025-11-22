using UnityEngine;
using UnityEngine.InputSystem;

namespace Asasingame.Core.Airplane.Runtimes
{
    public class JetAnimationController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Animator animator;
        [SerializeField] private AirplaneController airPlaneController;
        [SerializeField] private PlayerInput playerInput;

        [Header("Properties")]
        [SerializeField] private float damping;

        private Vector2 inputVector;

        private void Update()
        {
            inputVector = playerInput.actions["Move"].ReadValue<Vector2>();
            animator.SetFloat("Vertical", inputVector.y, damping, Time.deltaTime);
            animator.SetFloat("Horizontal", inputVector.x, damping, Time.deltaTime);
        }
    }
}


