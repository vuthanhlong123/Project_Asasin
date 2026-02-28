using INab.Dissolve;
using UnityEngine;

namespace Asasingame.Common.ShaderGraphs
{
    public class CustomDissovleControl : MonoBehaviour
    {
        public enum ControlType
        {
            Dissolve,
            Materialize
        }

        [SerializeField] private Dissolver dissolver;
        [SerializeField] private bool runAtStart;
        [SerializeField] private float delay;
        [SerializeField] private ControlType startType;

        private void Start()
        {
            if (runAtStart)
            {
                if (delay > 0)
                    Invoke(nameof(Run), delay);
                else Run();
            }
        }

        private void Run()
        {
            if(startType == ControlType.Materialize)
                dissolver.Materialize();
            else if(startType == ControlType.Dissolve)
                dissolver.Dissolve();
        }
    }
}


