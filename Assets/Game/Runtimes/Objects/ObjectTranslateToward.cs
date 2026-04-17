using UnityEngine;

namespace Game.Runtimes.Objects
{
    public class ObjectTranslateToward : MonoBehaviour
    {
        [SerializeField] private float speed;

        private void Update()
        {
            transform.position += transform.forward * speed * Time.deltaTime;
            //transform.Translate(transform.forward *speed * Time.deltaTime);
        }
    }
}


