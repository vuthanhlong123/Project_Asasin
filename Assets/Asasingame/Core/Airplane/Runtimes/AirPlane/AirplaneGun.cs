using UnityEngine;
using UnityEngine.InputSystem;

namespace Asasingame.Core.Airplane.Runtimes
{
    public class AirplaneGun : AirplaneWeaponUnit
    {
        [SerializeField] private float fireRate;

        [Header("References")]
        [SerializeField] private Transform airplaneHead;
        [SerializeField] private AirplaneCamera airplaneCamera;

        [Header("Projectile")]
        [SerializeField] private GameObject projectile;

        [Header("Input")]
        [SerializeField] private PlayerInput playerInput;

        [Header("Sound")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip audioClip;

        private bool isFire = false;
        private bool isCooldowned = false;
        private Transform cameraTrans;

        protected override void Awake()
        {
            base.Awake();

            isReady = true;
        }

        private void Start()
        {
            cameraTrans = Camera.main.transform;
        }

        private void AirplaneGun_started(InputAction.CallbackContext obj)
        {
            isFire = true;
        }

        private void AirplaneGun_canceled(InputAction.CallbackContext obj)
        {
            isFire = false;
        }

        private void Update()
        {
            if (availableBullet < 0 && data.BulletMode == Runtimes.Data.BulletMode.Limit) return;

            if (isFire && isCooldowned)
            {
                if(audioSource && audioClip)
                    audioSource.PlayOneShot(audioClip);

                CreateProjectile();
                OnFireEvent();

                isCooldowned = false;
                Invoke(nameof(CoolDownAvailable), fireRate);

                if(availableBullet <=0 && data.BulletMode == Runtimes.Data.BulletMode.Limit)
                {
                    isReady = false;
                }
            }
        }

        private void CreateProjectile()
        {
            if (airplaneCamera.IsAimming)
            {
                Vector3 targetPoint = cameraTrans.position + cameraTrans.forward * 1000f;
                Vector3 shootDirection = (targetPoint - transform.position).normalized;
                GameObject newProjectile = Instantiate(projectile, transform.position, Quaternion.LookRotation(shootDirection, Vector3.up));// transform.rotation); 
            }
            else
            {
                Vector3 targetPoint = airplaneHead.position + airplaneHead.forward * 1000f;
                Vector3 shootDirection = (targetPoint - transform.position).normalized;
                GameObject newProjectile = Instantiate(projectile, transform.position, Quaternion.LookRotation(shootDirection, Vector3.up));
            }

            if(data.BulletMode == Runtimes.Data.BulletMode.Limit)
                availableBullet--;
        }

        private void CoolDownAvailable()
        {
            isCooldowned = true;
        }

        private void OnEnable()
        {
            playerInput.actions["Fire"].started += AirplaneGun_started;
            playerInput.actions["Fire"].canceled += AirplaneGun_canceled;
            isCooldowned = true;
        }

        private void OnDisable()
        {
            playerInput.actions["Fire"].started -= AirplaneGun_started;
            playerInput.actions["Fire"].canceled -= AirplaneGun_canceled;
        }
    }
}


