using UnityEngine;

namespace Asasingame.Core.Object.Runtimes
{
    public class ObjectSetActive : MonoBehaviour
    {
        [SerializeField] private float activeDelayTime;
        [SerializeField] private bool state;
        [SerializeField] private GameObject target;

        private void Start()
        {
            if(activeDelayTime>0)
            {
                Invoke(nameof(Run), activeDelayTime);
            }
            else
            {
                Run();
            }
        }

        private void Run()
        {
            if(target)
                target.SetActive(state);
        }
    }

}

