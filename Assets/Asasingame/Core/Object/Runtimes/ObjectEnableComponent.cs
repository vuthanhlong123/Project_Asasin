using UnityEngine;

namespace Asasingame.Core.Object
{
    public class ObjectEnableComponent : MonoBehaviour
    {
        [SerializeField] private float startDelayTime;
        [SerializeField] private bool state;
        [SerializeField] private Behaviour target;

        private void Start()
        {
            if(startDelayTime > 0) 
                Invoke(nameof(Run), startDelayTime);
            else Run();
        }

        private void Run()
        {
            if(target)
            {
                target.enabled = state;
            }
        }
    }
}


