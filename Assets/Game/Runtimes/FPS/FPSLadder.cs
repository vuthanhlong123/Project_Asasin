using Game.Runtimes.Ultility;
using UnityEngine;

namespace Game.Runtimes.FPS
{
    public class FPSLadder : MonoBehaviour
    {
        public enum DefaultLookDirection
        {
            Foward,
            Back,
            Right,
            Left,
            Up,
            Down
        }

        [SerializeField] private Vector3 topEndPoint;
        [SerializeField] private Vector3 topPoint;
        [SerializeField] private Vector3 bottomPoint;
        [SerializeField] private LayerMask availableClimberLayer;
        [SerializeField] private DefaultLookDirection forwardDirection;

        public void Execute(GameObject target)
        {
            if (!GameUltility.IsInLayerMask(target.layer, availableClimberLayer)) return;

            var fpsController = target.GetComponent<CustomFPSController>();

            Vector3 rotatedOffset = transform.rotation * topPoint;
            Vector3 finalTopPos = transform.position + rotatedOffset;

            rotatedOffset = transform.rotation * topEndPoint;
            Vector3 finalTopEndPos = transform.position + rotatedOffset;

            rotatedOffset = transform.rotation * bottomPoint;
            Vector3 finalBottomPos = transform.position + rotatedOffset;
            fpsController._ClimbAbility.StartAbility(finalTopPos, finalTopEndPos, finalBottomPos, GetForwardDirection(forwardDirection));
        }

        private Vector3 GetForwardDirection(DefaultLookDirection forwardDirection)
        {
            switch (forwardDirection)
            {
                case DefaultLookDirection.Left: return -transform.right;
                case DefaultLookDirection.Right: return transform.right;
                case DefaultLookDirection.Up: return transform.up;
                case DefaultLookDirection.Down: return -transform.up;
                case DefaultLookDirection.Back: return -transform.forward;
            }

            return transform.forward;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Vector3 rotatedOffset = transform.rotation * topPoint;
            Vector3 spherePos1 = transform.position + rotatedOffset;
            Gizmos.DrawSphere(spherePos1, 0.2f);

            rotatedOffset = transform.rotation * bottomPoint;
            Vector3 spherePos2 = transform.position + rotatedOffset;
            Gizmos.DrawSphere(spherePos2, 0.2f);

            Gizmos.DrawLine(spherePos1, spherePos2);

            rotatedOffset = transform.rotation * topEndPoint;
            spherePos2 = transform.position + rotatedOffset;
            Gizmos.DrawSphere(spherePos2, 0.2f);
        }
#endif
    }
}


