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

        [SerializeField] private float timeScale = 1;
        [SerializeField] private Stack[] stacks;

        private void Start()
        {
            foreach (var stack in stacks)
            {
                StartCoroutine(Run(stack));
            }

        }

        [ContextMenu("Restart")]
        public void Restart()
        {
            foreach (var stack in stacks)
            {
                StartCoroutine(Run(stack));
            }
        }

        private IEnumerator Run(Stack stack)
        {
            if (timeScale < 1) timeScale = 1;

            if(stack.delay > 0) 
                yield return new WaitForSeconds(stack.delay/ timeScale);

            float start = Time.time;
            while(Time.time - start <= stack.duration/ timeScale)
            {
                stack.mat.SetFloat(stack.propertyName, Mathf.Lerp(stack.range.x, stack.range.y, (Time.time - start) / (stack.duration/timeScale)));
                yield return null;
            }
        }
    }
}


