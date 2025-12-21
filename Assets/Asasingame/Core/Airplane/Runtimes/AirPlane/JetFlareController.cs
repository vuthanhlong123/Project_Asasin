using Asasingame.Core.Airplane.Runtimes.UIs;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Asasingame.Core.Airplane.Runtimes
{
    public class JetFlareController : MonoBehaviour
    {
        [SerializeField] private AirplaneController airplaneController;
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private JetFlare flareSample;
        [SerializeField] private int flarePerLaunchTime;
        [SerializeField] private float launchInterval;
        [SerializeField] private float launchCooldown;
        [SerializeField] private float addedSpeed;
        [SerializeField] private Vector2 horizontalError;
        [SerializeField] private Vector2 VerticaleError;

        [Header("UI")]
        [SerializeField] private UIFlare uiFlare;

        private bool isAvailable;
        private bool isLaunching;
        private int lauchedAmount;

        private void Start()
        {
            playerInput.actions["Flare"].performed += JetFlareController_performed;
            LaunchAvailable();
        }

        private void JetFlareController_performed(InputAction.CallbackContext obj)
        {
            if (isLaunching || isAvailable == false) return;

            isAvailable = false;
            Invoke(nameof(LaunchAvailable), launchCooldown);

            isLaunching = true;
            lauchedAmount = 0;
            InvokeRepeating(nameof(LaunchFlare),0, launchInterval);

            if(uiFlare)
            {
                uiFlare.SetUnAvailable();
            }
        }

        private void LaunchFlare()
        {
            JetFlare flareInstance1 = Instantiate(flareSample, transform.position, transform. rotation);
            JetFlare flareInstance2 = Instantiate(flareSample, transform.position, transform.rotation);

            float horizontal = Random.Range(horizontalError.x, horizontalError.y);
            float verticle = Random.Range(VerticaleError.x, VerticaleError.y);

            Vector3 direction1 = transform.forward + (transform.right / horizontal) + (transform.up / verticle);
            Vector3 direction2 = transform.forward + (-transform.right / horizontal) + (transform.up / verticle);

            flareInstance1.SetValue(direction1, airplaneController.CurrentSpeed() + Random.Range(0, addedSpeed));
            flareInstance2.SetValue(direction2, airplaneController.CurrentSpeed() + Random.Range(0, addedSpeed));

            lauchedAmount++;
            if (lauchedAmount >= flarePerLaunchTime)
            {
                isLaunching= false;
                CancelInvoke(nameof(LaunchFlare));
            }
        }

        private void LaunchAvailable()
        {
            isAvailable = true;
            if (uiFlare)
            {
                uiFlare.SetAvailable();
            }
        }
    }
}


