using UnityEngine;

namespace Asasingame.Core.Object
{
    public class ObjectRotationLoop : MonoBehaviour
    {
        [SerializeField] private Vector3 axis;

        private void Update()
        {
            transform.Rotate(axis*Time.deltaTime);
        }
    }
}


