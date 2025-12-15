using Unity.Transforms;
using UnityEngine;

namespace Asasingame.Core.Airplane.Runtimes
{
    public class Rocket : MonoBehaviour
    {
        [SerializeField] private float lifeTime;
        [SerializeField] private float maxSpeed;
        [SerializeField] private float launchDelay;
        [SerializeField] private float acceleration;
        [SerializeField] private float gravity;
        [SerializeField] private ParticleSystem[] particleSystems;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip launchSound;

        private float currentSpeed;
        private bool isSpeedUp;

        private Vector3 direction;
        private Rigidbody _rigi;

        private void FixedUpdate()
        {
            if (_rigi == null) return;

            if (currentSpeed < maxSpeed && isSpeedUp)
            {
                currentSpeed += acceleration * Time.fixedDeltaTime;
            }

            if (!isSpeedUp)
            {
                _rigi.linearVelocity = ((direction * currentSpeed) + (-transform.up * gravity));
            }
            else
            {
                _rigi.linearVelocity = direction * currentSpeed;
                _rigi.MoveRotation(Quaternion.LookRotation(direction, Vector3.up));
            }

           // UpdateLaunchFX();
        }

        public void SetValue(Vector3 direction, float startSpeed)
        {
            this.direction = direction;
            this.currentSpeed = startSpeed;
        }

        public void OnDead()
        {
            Destroy(gameObject);
        }

        private void OnEnable()
        {
            
        }

        public void DeActive()
        {
            this.enabled = false;
            DisableLaunchFX();
        }

        public void Launch()
        {
            Quaternion worldRot = transform.rotation;
            transform.parent = null;
            transform.rotation = worldRot;

            _rigi = GetComponent<Rigidbody>();
            _rigi.isKinematic = false;
            _rigi.interpolation = RigidbodyInterpolation.Interpolate;
            Invoke(nameof(OnDead), lifeTime);
            Invoke(nameof(SetSpeedUp), launchDelay);

            isSpeedUp = false;
            audioSource.PlayOneShot(launchSound);

            this.enabled = true;
        }

        private void SetSpeedUp()
        {
            isSpeedUp = true;
            EnableLaunchFX();
            audioSource.Play();
        }

        private void EnableLaunchFX()
        {
            foreach(var particle in particleSystems)
            {
                particle.Clear();
                particle.Play();
            }
        }

        private void DisableLaunchFX()
        {
            foreach (var particle in particleSystems)
            {
                particle.Stop();
            }
        }

        private void UpdateLaunchFX()
        {
            foreach (var particle in particleSystems)
            {
                particle.Simulate(Time.fixedDeltaTime, true, false);
            }
        }
    }

}

