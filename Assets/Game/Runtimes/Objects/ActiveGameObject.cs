using UnityEngine;

namespace Game.Runtimes.Objects
{

    public class ActiveGameObject : MonoBehaviour
    {
        [SerializeField] private bool runOnStart;
        [SerializeField] private float delay;
        [SerializeField] private bool value;
        [SerializeField] private GameObject target;

        private void Start()
        {
            if (!runOnStart) return;

            Invoke(nameof(Execute), delay);
        }

        public void Execute()
        {
            if (value) Active(); else DeActive();
        }

        public void Active()
        {
            if (target != null)
            {
                target.SetActive(true);
            }
        }

        public void DeActive()
        {
            if (target != null)
            {
                target.SetActive(false);
            }
        }
    }

}
