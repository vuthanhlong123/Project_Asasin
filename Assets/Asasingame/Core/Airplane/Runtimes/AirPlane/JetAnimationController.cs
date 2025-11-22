using UnityEngine;

namespace Asasingame.Core.Airplane.Runtimes
{
    public class JetAnimationController : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private AirplaneController airPlaneController;

        private void Update()
        {
            animator.SetFloat("Vertical", airPlaneController.InputV);
            animator.SetFloat("Horizontal", airPlaneController.InputH);
        }
    }
}


