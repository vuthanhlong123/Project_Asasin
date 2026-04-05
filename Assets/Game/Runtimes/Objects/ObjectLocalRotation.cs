using UnityEngine;

namespace Game.Runtimes.Objects
{
    public class ObjectLocalRotation : MonoBehaviour
    {
        [SerializeField] private Vector3 euler;

        private void Update()
        {
            transform.Rotate(euler, Space.Self);
        }
    }
}


