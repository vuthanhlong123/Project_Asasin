using System.Collections;
using UnityEngine;
using UnityEngine.Splines;

namespace Game.Runtimes.NPC.Movement
{
    public class NPCSplineMovement : MonoBehaviour, INPCMovement
    {
        [SerializeField] private SplineContainer splineContainer;
        [SerializeField] private float moveSpeed;
        [SerializeField] private float acceleration;
        [SerializeField] private float angulerSpeed;
        [SerializeField] private float stopProgressValue;

        private float currentSplineProgress;
        private float currentMoveSpeed;

        private void Start()
        {
            InitPosition();

            this.enabled = false;
        }

        private void Update()
        {
            if (splineContainer == null) return;

            if(currentSplineProgress < stopProgressValue)
            {
                currentMoveSpeed += Time.deltaTime * acceleration;
            }
            else
            {
                currentMoveSpeed -= Time.deltaTime * acceleration;
            }
            currentMoveSpeed = Mathf.Clamp(currentMoveSpeed, 0, moveSpeed);

            currentSplineProgress += Time.deltaTime * currentMoveSpeed;
            if (currentSplineProgress > 1)
            {
                currentMoveSpeed = 0;
                this.enabled = false;
            }

            transform.position = splineContainer.EvaluatePosition(currentSplineProgress);

            Vector3 direction = splineContainer.EvaluateTangent(currentSplineProgress);
            transform.rotation = Quaternion.Lerp(transform.rotation,  Quaternion.LookRotation(direction), (currentMoveSpeed /moveSpeed) * angulerSpeed);
        }

        public void EnableMovement()
        {
            this.enabled = true;
            currentSplineProgress = 0;
            currentMoveSpeed = 0;
        }

        public void InitPosition()
        {
            transform.position = splineContainer.EvaluatePosition(0);
        }

        public float Forward()
        {
            return currentMoveSpeed / moveSpeed;
        }

        public void Rotate(Vector3 euler, float duration)
        {
            if(HandleRotateCoroutine != null)
            {
                StopCoroutine(HandleRotateCoroutine);
            }

            HandleRotateCoroutine = StartCoroutine(HandleRotate(euler, duration));
        }

        private Coroutine HandleRotateCoroutine;

        private IEnumerator HandleRotate(Vector3 euler, float duration)
        {
            float start = Time.time;
            Vector3 startEuler = transform.eulerAngles;
            while (Time.time - start < duration)
            {
                yield return null;
                float angelX = Mathf.LerpAngle(startEuler.x, euler.x, (Time.time - start) / duration);
                float angelY = Mathf.LerpAngle(startEuler.y, euler.y, (Time.time - start) / duration);
                float angelZ = Mathf.LerpAngle(startEuler.z, euler.z, (Time.time - start) / duration);

                transform.eulerAngles = new Vector3(angelX, angelY, angelZ);
            }
        }
    }
}


