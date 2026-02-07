using System;
using System.Collections;
using UnityEngine;

namespace Asasingame.Common.ShaderGraphs
{
    public class UpdatePropertyFloatOneShot : MonoBehaviour
    {
        [Serializable]
        public class Stack
        {
            public Material mat;
            public string propertyName;
            public Vector2 range;
            public float delay;
            public float duration;
        }

        [SerializeField] private Stack[] stacks;

        private void Start()
        {
            foreach (var stack in stacks)
            {
                StartCoroutine(Run(stack));
            }
        }

        private IEnumerator Run(Stack stack)
        {
            yield return new WaitForSeconds(stack.delay);

            float start = Time.time;
            while(Time.time - start <= stack.duration)
            {
                stack.mat.SetFloat(stack.propertyName, Mathf.Lerp(stack.range.x, stack.range.y, (Time.time - start) / stack.duration));
                yield return null;
            }
        }
    }
}


