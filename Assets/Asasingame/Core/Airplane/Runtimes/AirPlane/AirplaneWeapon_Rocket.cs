using UnityEngine;

namespace Asasingame.Core.Airplane.Runtimes
{
    public class AirplaneWeapon_Rocket : AirplaneWeaponUnit
    {
        [Header("Spawning")]
        [SerializeField] private Rocket rocketSample;
        [SerializeField] private Vector3 spawnOffset;

        [Header("Values")]
        [SerializeField] private float cooldown;

        private Rocket currentRocketInstance;

        protected override void Awake()
        {
            isReady = true;
            CreateRocket();
        }

        public override void Active()
        {
            base.Active();

            if (manager == null) return;
            manager.PlayerInput.actions["Fire"].performed += AirplaneWeapon_Rocket_performed;
        }

        private void AirplaneWeapon_Rocket_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if(isReady == false) return;
            Fire();
        }

        private void Fire()
        {
            if (currentRocketInstance == null) return;

            float airplaneSpeed = manager.AirplaneController.CurrentSpeed();
            currentRocketInstance.SetValue(transform.forward, airplaneSpeed);
            currentRocketInstance.Launch();
            currentRocketInstance = null;

            isReady = false;
            Invoke(nameof(OnCooldown), cooldown);
        }

        public override void DeActive()
        {
            base.DeActive();

            if (manager == null) return;
            manager.PlayerInput.actions["Fire"].performed -= AirplaneWeapon_Rocket_performed;
        }

        private void CreateRocket()
        {
            currentRocketInstance = Instantiate(rocketSample, this.transform);
            currentRocketInstance.transform.localPosition = spawnOffset;
            currentRocketInstance.transform.rotation = transform.rotation;

            currentRocketInstance.DeActive();
        }

        private void OnCooldown()
        {
            CreateRocket();
            SetReady();
        }
    }
}


