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
        private Transform camereTrans;

        protected override void Awake()
        {
            base.Awake();
            isReady = true;
            CreateRocket();
        }

        private void Start()
        {
            camereTrans = Camera.main.transform;
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
            manager.ChangeToOtherReadyWeaponUnit(ID);
        }

        private void Fire()
        {
            if (currentRocketInstance == null || availableBullet <=0) return;
            availableBullet--;

            Vector3 direction = Vector3.zero;
            if (manager.AirplaneCamera.IsAimming)
            {
                direction = ((camereTrans.position+ camereTrans.forward * 1000) - transform.position).normalized; 
            }
            else
            {
                direction = manager.AirplaneController.transform.forward;
            }

            float airplaneSpeed = manager.AirplaneController.CurrentSpeed();
            currentRocketInstance.SetValue(direction, airplaneSpeed);
            currentRocketInstance.Launch();
            currentRocketInstance = null;

            isReady = false;
            Invoke(nameof(OnCooldown), cooldown);

            OnFireEvent();
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
            availableBullet = data.BulletAmount;
            CreateRocket();
            SetReady();
            OnReloadedEvent();
        }
    }
}


