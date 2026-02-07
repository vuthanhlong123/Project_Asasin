using System;
using UnityEngine;

namespace Asasingame.Common.ShaderGraphs
{
    public class UpdatePropertyTransform : MonoBehaviour
    {
        [Serializable]
        public class Stack
        {
            [SerializeField] private TransformType type;
            [SerializeField] private Material material;
            [SerializeField] private string propertyName;
            [SerializeField] private Transform obj;

            public void Update()
            {
                if (obj == null) return;

                switch (type)
                {
                    case TransformType.Forward: UpdateForward(); break;
                    case TransformType.WorldPosition: UpdateWorldPosition(); break;

                }
            }

            private void UpdateForward()
            {
                material.SetVector(propertyName, obj.forward);
            }

            private void UpdateWorldPosition()
            {
                material.SetVector(propertyName, obj.position);
            }
        }

        public enum TransformType
        {
            LocalPosition,
            WorldPosition,
            Forward,
            Right,
            Up,
            Down
        }

        [SerializeField] private Stack[] stacks;
        

        private void Update()
        {
            foreach (var stack in stacks)
            {
                stack.Update();
            }
        }
    }
}
